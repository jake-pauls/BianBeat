import onnx
import onnxruntime as ort
import torch
import torch.nn as nn
import torch.onnx

from expression_dataset import ExpressionDataset
from model import ExpressionMLP
from torch.utils.data import DataLoader
from torch.utils.data import random_split

# Learning parameters
total_epochs = 60
lr = 1e-3

# Load the dataset (five frames per input, 255 features)
num_input_features = 255
dataset = ExpressionDataset("data/mediapipe_expression_dataset_five_frames.csv")

# Split into training and validation to ensure this is actually converging
train_size = int(0.8 * len(dataset))
val_size = len(dataset) - train_size
train_set, val_set = random_split(dataset, [train_size, val_size])

train_loader = DataLoader(train_set, batch_size=64, shuffle=True)
val_loader = DataLoader(val_set, batch_size=64)

# Create the model
model = ExpressionMLP(255, 5)
optimizer = torch.optim.Adam(model.parameters(), lr=lr)
loss_fn = nn.CrossEntropyLoss()

# Training
for epoch in range(total_epochs):
    model.train()
    total_loss = 0

    for features, labels in train_loader:
        optimizer.zero_grad()
        output = model(features)
        loss = loss_fn(output, labels)
        loss.backward()
        optimizer.step()

        total_loss += loss

    avg_loss = total_loss / len(train_loader)
    print(f"Epoch: {epoch} total_loss={total_loss:.3f} avg_loss={avg_loss:.3f}")

"""
# Validation

model.eval()
val_loss = 0

with torch.no_grad():
    for features, labels in val_loader:
        outputs = model(features)
        loss = loss_fn(outputs, labels)
        val_loss += loss.item()

        # Checking predictions per batch
        # preds = outputs.argmax(dim=1)
        # print(f"pred: {preds[:10]}")
        # print(f"true: {labels[:10]}")

val_loss /= len(val_loader)
print(f"val_loss: {val_loss}")
"""

# Export the model in inference mode
model = model.float()
model.eval()
for p in model.parameters():
    p.requires_grad = False

input_dim = dataset.features.shape[1]
dummy_input = torch.zeros(1, input_dim, dtype=torch.float32)
torch.onnx.export(
    model,
    dummy_input,
    "blem_model.onnx",
    input_names=["input"],
    output_names=["logits"],
    opset_version=13,
    do_constant_folding=True,
    external_data=False
)

# Validate the exported model
onnx_model = onnx.load("blem_model.onnx")
onnx.checker.check_model(onnx_model)

session = ort.InferenceSession("blem_model.onnx")

test_input = { "input": dummy_input.numpy() }
logits = session.run(None, test_input)[0]
print(f"Shape of the data output by the model: {logits.shape}")

# This checks if the model will reference any of the sidecar data in the *.onnx.data file.
# If this output is 'False' that file is not required to run inference onn the model.
uses_external_data = any(
    t.HasField("data_location") and t.data_location == onnx.TensorProto.EXTERNAL
    for t in onnx_model.graph.initializer
)
print(f"Uses external/sidecar data: {uses_external_data}")
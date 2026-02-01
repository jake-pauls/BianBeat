import torch.nn as nn
import torch.nn.functional as F

class ExpressionMLP(nn.Module):
    """
    I'm very bad at ML stuff - so this is about as dead simple as "ML stuff" gets (I think).

    So, I'm going to try this without exempting any features from my self-captured data set first.

    Therefore, I have:
    - 51 input features from the MediaPipe API (I removed the '_neutral' feature)
    - These need to output to 5 possible classes
        1. Neutral
        2. Happy
        3. Sad
        4. Anger
        5. Shocked
    """
    def __init__(self, num_input_features, num_output_labels):
        super().__init__()

        # Not quite sure if this is the best layout for layers...
        half_num_input_features = int(num_input_features / 2)
        quarter_num_input_features = int(num_input_features / 4)

        # Three full-connected dense layers        
        self.fc1 = nn.Linear(num_input_features, half_num_input_features)
        self.fc2 = nn.Linear(half_num_input_features, quarter_num_input_features)         
        self.fc3 = nn.Linear(quarter_num_input_features, num_output_labels)  # Output one of 5 labels
    
    def forward(self, x):
        # Propogate the input throughout the layers
        x = F.relu(self.fc1(x))
        x = F.relu(self.fc2(x))
        # Passthrough, let the training code provide a loss function
        return self.fc3(x)
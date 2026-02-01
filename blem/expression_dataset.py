import torch
import pandas as pd

from torch.utils.data import Dataset

# Note: These must match the expressions we check in 'Assets/Scripts/FaceDetection/Expression'
LABEL_MAP = {
    "Neutral": 0,
    "Happy": 1,
    "Sad": 2,
    "Angry": 3,
    "Shocked": 4
}

class ExpressionDataset(Dataset):
    """
    Creates a PyTorch compatible representation of the dataset exported by the ExpressionSampleExporter 
    in the Unity project. 
    
    An example of this dataset is committed into the repo in 'data/mediapipe_emotion_dataset.csv'.
    """
    def __init__(self, csv_path):
        df = pd.read_csv(csv_path)
        # Drop the timestamp column - not using it here
        df = df.drop(columns=["timestamp"])
        # Convert the labels to integers
        self.labels = df["label"].map(LABEL_MAP).values
        # Drop label column so we only have the features
        self.features = df.drop(columns=["label"]).values

        # Convert to tensors
        self.features = torch.tensor(self.features, dtype=torch.float32)
        self.labels = torch.tensor(self.labels, dtype=torch.long)
    
    def __len__(self):
        return len(self.labels)
    
    def __getitem__(self, index):
        return self.features[index], self.labels[index]
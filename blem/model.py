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
    def __init__(self):
        super().__init__()
        # Three full-connected dense layers        
        self.fc1 = nn.Linear(51, 32) # Input is 51 features
        self.fc2 = nn.Linear(32, 16) # Intermediate layer so that we don't go from 51 straight to 5 
        self.fc3 = nn.Linear(16, 5)  # Output one of 5 labels
    
    def forward(self, x):
        # Propogate the input throughout the layers
        x = F.relu(self.fc1(x))
        x = F.relu(self.fc2(x))
        # Passthrough, let the training code provide a loss function
        return self.fc3(x)
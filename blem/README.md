# BLEM
**Very** ad-hoc and extremely tiny neural network designed for emotion detection using data output from the [MediaPipe API](https://github.com/homuler/MediaPipeUnityPlugin) in Unity.

⚠️ Yellow warning tape included. Use with discretion. ⚠️

## Installation
```
# After setting up a virtual environment, or, directly on your machine
pip install -r requirements.txt

# Train the model using the provided data, will also export to a Unity-friendly .onnx file
python ./train.py
```

## Dataset
The self-created datasets used had 51 input features provided from the API in a single frame call. In our self-generated datasets these yielded data mapping to 5 different output labels.

Our inference was improved after expanding the available input features to 255, accounting for five frames per-inference.
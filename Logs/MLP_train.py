import torch
import torch.nn as nn
import torch.optim as optim
from mlagents_envs.environment import UnityEnvironment
import numpy as np

# Define the MLP Model
class MLPModel(nn.Module):
    def __init__(self, input_dim, hidden_dim, output_dim):
        super(MLPModel, self).__init__()
        self.model = nn.Sequential(
            nn.Linear(input_dim, hidden_dim),
            nn.ReLU(),
            nn.Linear(hidden_dim, hidden_dim),
            nn.ReLU(),
            nn.Linear(hidden_dim, output_dim),
        )

    def forward(self, x):
        return self.model(x)

# Initialize Unity Environment and Reset Logic
class UnityEnvironmentHandler:
    def __init__(self, env_path, reset_channel_id="6a50f81c-2c74-4f17-b35e-2ab5adf8b174"):
        self.env = UnityEnvironment(file_name=env_path)
        self.behavior_name = None

    def initialize(self):
        self.env.reset()
        self.behavior_name = list(self.env.behavior_specs.keys())[0]

    def reset_environment(self):
        self.env.reset()

    def step(self, actions):
        self.env.set_actions(self.behavior_name, actions)
        self.env.step()
        decision_steps, terminal_steps = self.env.get_steps(self.behavior_name)
        return decision_steps, terminal_steps

# Combined Training and Reset Script
def train_model(env_handler, model, num_episodes=1000, lr=0.001):
    optimizer = optim.Adam(model.parameters(), lr=lr)
    criterion = nn.MSELoss()

    for episode in range(num_episodes):
        print(f"Episode {episode + 1}/{num_episodes}")
        
        # Reset environment
        env_handler.reset_environment()
        
        decision_steps, terminal_steps = env_handler.env.get_steps(env_handler.behavior_name)
        if len(decision_steps) == 0:
            continue

        # Collect observations from Unity
        state = decision_steps.obs[0][0]  # The observation from Unity (9-dimensional vector)
        
        # Convert state to tensor (input_dim = 9)
        state_tensor = torch.tensor(state, dtype=torch.float32).unsqueeze(0)  # Add batch dimension
        
        done = False
        while not done:
            # Forward pass through the model to predict target position
            predicted_target = model(state_tensor)
            
            # Perform action in Unity environment
            decision_steps, terminal_steps = env_handler.step(predicted_target.detach().numpy())

            if len(terminal_steps) > 0:
                done = True
                reward = terminal_steps.reward[0]
            else:
                reward = decision_steps.reward[0]
                next_state = decision_steps.obs[0][0]
                true_target = decision_steps.obs[1][0]  # Assuming the true target is in the second observation

                # Compute loss between predicted and true target
                target_tensor = torch.tensor(true_target, dtype=torch.float32).unsqueeze(0)  # Add batch dimension
                loss = criterion(predicted_target, target_tensor)
                
                # Update model
                optimizer.zero_grad()
                loss.backward()
                optimizer.step()

        print(f"Episode {episode + 1} finished.")

    print("Training complete!")

# Main Script
if __name__ == "__main__":
    #env_path = "/home/lifan/colcon_ws/build/"  # Path to your Unity executable/  path_to_your_unity_env
    env_path = "/home/lifan/colcon_ws/src/smarc2/simulation/SMARCUnityAssets/Runtime/Prefabs"  # Path to your Unity executable/  path_to_your_unity_env
    input_dim = 9  # [x, y, z, vx, vy, vz, tx, ty, tz]
    hidden_dim = 128
    output_dim = 3  # Target position (x, y, z)

    # Initialize environment and model
    env_handler = UnityEnvironmentHandler(env_path)
    env_handler.initialize()
    model = MLPModel(input_dim, hidden_dim, output_dim)

    # Train the model
    train_model(env_handler, model, num_episodes=1000)

    # Close Unity environment
    env_handler.env.close()

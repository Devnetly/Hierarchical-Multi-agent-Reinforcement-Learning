`conda create -n mlagents python=3.10.12 && conda activate mlagents`

`pip install mlagents==1.1.0`

`pip install mlagents_envs==1.1.0`

`mlagents-learn config/puzzle_config1.yaml --env=build/puzzle --run-id=puzzle_expirement_3 --torch-device=cuda --resume`
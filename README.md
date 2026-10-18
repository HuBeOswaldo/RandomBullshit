# RandomBullshit

A community-focused Valheim mod in active development. Future updates will
respond to community requests and aim to make the game a better experience.

## Requirements

- Valheim
- BepInExPack Valheim
- Jotunn (Valheim Library)

## Features

- T.W.I.G. and TWM (Training with Mexicanito) training dummies display a
  compact damage-per-second number above them, calculated over a rolling
  5-second window. Each attacker sees their own damage locally, including when
  hitting a dummy placed by another player.
- TWM is a passive training dummy available in the hammer build menu.
- T.W.I.G. and T.W.M. cannot be destroyed and regenerate to full health after
  5 seconds without damage; damage does not drop their build materials.
- Dismantling a dummy still returns its build materials.
- DPS resets after 3 seconds without damage, and the label disappears after
  5 seconds without a hit.

## Examples

### T.W.I.G.

![T.W.I.G. training dummy](https://raw.githubusercontent.com/HuBeOswaldo/RandomBullshit/main/images/twig-example.jpg)

### T.W.M.

![T.W.M. training dummy](https://raw.githubusercontent.com/HuBeOswaldo/RandomBullshit/main/images/twm.png)

## Usage

Build either training dummy from the hammer menu and hit it to see its current
DPS displayed above the dummy.

If a dummy gets stuck, use `/removedummy` in chat to remove the nearest owned
training dummy within 8 meters. This emergency command does not return materials.

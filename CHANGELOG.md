## 1.4.0

- Added config for act multiplier. Increasing this allows more difficult enemies and more enemies to spawn
- Added small chance for a duplicate enemy, since it's rare to get duplicates
- Add a rule for Fox to prevent unbalanced encounters (too little or many enemies)
- Tweaked the balance of enemy/act weights

## 1.3.1

- Updated the rule for "matching previous encounter" to now prevent any formation that's been seen for the current run, instead of only the previous battle
  - This will mostly affect solo encounters as they are easily noticable when they repeat, and have higher chances to do so
- Add a rule to prevent matching any enemy type seen the previous battle
  - This will prevent the next encounter being only 1-2 enemies that are different, and provide more variety in general

## 1.3.0

- Adjusted weights for each enemy to give them a value for each act. This will allow scaling enemies to be powerful later, without having a weak encounter earlier on
- Allow enemies to spawn in random slots
- Further adjustments of enemy/act weights and rng

## 1.2.3

- Adjusted rng to have less chances for solo encounters at certain weights
- Further adjustments of enemy/act weights

## 1.2.2

- Fixed issue with enemies that didn't have a weight value, softlocking the game when they died

## 1.2.1

- Adjusted power gain to be based on the enemy's weight and act weight
- Further adjustments of enemy/act weights

## 1.2.0

- Fixed Sunny to not softlock, and make the 3 fairies use their spellcard 1st turn
- Added custom rules to prevent weird encounter behaviour and weak solo encounters
- Added a custom 8 slot formation, to have 3 summon exclusive slots
- Firepower gain on enemies becomes delayed until the end of all enemies turns
- Star's lockon is also delayed now
- Added rule that prevents giving the same enemies as the previous encounter
- Further fine-tuning of weights and rng

## 1.1.0

- Added a "first enemy minimum weight" for possible encounters  
- Added a maximum enemy limit between 3-5  
- The above changes should help with the problem of too many weaker enemies being chosen for encounters, and aoe being prominent  
- Adjusted weights for various enemies and act thresholds  

## 1.0.0

Initial release
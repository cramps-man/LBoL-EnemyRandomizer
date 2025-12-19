## Replaces normal and elite battle enemies with random enemies, based on act and weight value

Each enemy in the game is given a weight value based on difficulty.  
The total weight of all enemies for this battle can not go over the maximum, based on the current act section.  
Balance is still ongoing.  
If there are any enemy behavioural issues, feel free to comment.  

## Custom rules for invalid encounters

 - Support type units can not spawn solo
 - Can not be more than 1 summoner type unit
 - Gloomy kappa can not spawn without a drone or nitori
 - Can not match the enemies from the previous encounter

## Changes to vanilla game

 - Enemies that gain Firepower now gain Delayed Firepower, to prevent hidden damage mid enemy turn
 - Power gain is overridden and now scales based on the enemy's weight and the act weight. Overall power gain is probably a bit higher than vanilla

## For future

Add config to allow the act section maximum weights to be adjusted.  
Add configs regarding allowing elites.  
Other various misc things.  
# Retainers
Class to handle retainers for reborn buddy

Click basebot settings to change any of the defaults below.

# What it does:

- Goes to the nearest bell (you need to start it with a summoning bell in reasonable disctance, it won't teleport but will walk)
- Will interact with the bell, get your number of retainers then loop through them
  - If setting GetGil is true (default) it will get the retainer's gil
  - If setting DepositFromPlayer is true (default) it'll deposite any items you have into retainers that already have that item.
  - If DontOrganizeRetainers is false (default) then it will loop through the retainers 3 times total, getting their inventory and depositing yours then it checks for duplicates between retainers and if you have space it will move those items to your player's inventory and then loop through the retainers again depositing them to combine the stacks.
  
  If you don't have enough space in your inventory it will tell you so and stop. Make sure you have as many spaces as the number of moves it prints out and try again. If DontOrganizeRetainers is set to true it will just get the gill and deposit your items (if there are stack in a retainer). 
  
  If it can't find a summoning bell it will tell you, in that case go next to one and click the bell and close the ui and try pressing start again. 
  
  Leaving Debuglogging to false any major errors and status updates will still come up, if you turn it on it'll tell you each and every item as it loops through the retainers.

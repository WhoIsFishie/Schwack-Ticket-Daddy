# Welcome to Fishie's Schwack Listner!
its a bot coded to let you know as soon as tickets are up for sale on Schwack website so you can buy it asap.


# Getting started
to use this bot you will need to have a telegram account and also create a telegram bot token.
for starters you can download a built version of this bot on the release section or build from source yourself
rename the **example.appsettings.json** to  **appsettings.json** and open the file in notepad
set the **MovieName** when setting the movie name you dont need to enter the full movies name. you can just write one or 2 words that you are sure will be in the movie's title the bot will just check to see if the name you put is contained in the websites movie name list before notifying you

after this you will have to create a telegram bot and add it to a group chat and give the bot admin rights to the group chat
then obtain the group chat id 
put the groupchat id and bot token in the settings file 

now you are ready to launch the bot 
on launch the bot will send a test msg to your group to make sure notifications work. if you didn't get it then check troubleshoot section

## how to customize the delay
line 59
>     await Task.Delay(TimeSpan.FromMinutes(30));
edit this line currently it checks every 30 mins you can change the 30 to 15 to make it every 15 mins but then you will need to rebuild from source.
i didnt add a delay feature to settings cause i knew 90% of you will have it be 1 sec and end up ddosing the site and ruin this for everyone 

## can this auto book?
**yes but i have disabled the code for it.**
the code is there but its not called at any point in the public version. 
im sure i dont need to explain why it would be a dumb idea to make such a feature so accessible. 
> if you wish to enable the feature and abuse it thats on you.
> i take no responsibility since you will have to manually go through the code and enable it.

## Troubleshoot 

### cant get test notification
mention your bot in the groupchat and then open the following link
[api.telegram.org/bot{YOUR_BOT_TOKEN}/getUpdates](https://api.telegram.org/bot/getUpdates)
**remember to replace YOUR_BOT_TOKEN with your actual bot token** as you wrote it in the appsettings.json file

when the link opens look for something like this
>        "chat": {
>                    "id": -469696969,
>                    "title": "Gamer",
>                    "type": "group",
>                    "all_members_are_administrators": true
>                },

*make sure the title matches your group chat name*.  if it does take the id and put it in the *ChatId* section in your *appsettings.json* file

**yes you have to include the minus sign or it wont work**

### It didnt detect the movie
>  if (movie.Title.ToLower().Contains(settings.MovieName.ToLower()))

here is the code for detecting movie name
so as long as your spelling is correct and the name you gave is in the title on the website it should detect.

it would be a good idea to first test the bot on an already available movie to see its working as expected.

### I cant run it
you prolly need to install .net 7.0 runtime

### i cant build from source
download prebuilt from release section
if you have trouble building then it means you are too dumb to build it cause it works on my machine so no reason for it to not work on yours

## I found a bug
it works for me so you must have confused a feature with a bug 

## Auto Buy Tickets?
i disabled the code for it. if you want to you can enable it yourself. i take no responsibiliy for your actions!

## linux or mac verion wen?
pull the repo
open in vs code on your os of choice
make sure to restore nuget packages
hit compile 

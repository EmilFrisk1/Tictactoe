Tic-Tac-Toe WinForms Game
#Welcome to the Tic-Tac-Toe game developed using Windows Forms (WinForms). This README provides an overview of how to navigate and play the game.

##Getting Started
Main Menu: Upon launching the application, you are greeted with the main menu where you can choose your game mode:
Single Player (AI): Play against an AI opponent that uses the Minimax algorithm, making it challenging to win.
Two Players: Play against a friend on the same computer.
![App Screenshot](images/mainMenu.jpg)

##Player Selection
After selecting the game mode, you'll move to the player selection process:

Who are you?: This screen asks if you are a new or returning player.

New Player: If you're new, you'll be directed to create a new player profile.

Old Player: If you've played before, you can select an existing player.

Create New Player:

If you choose to create a new player, you'll be taken to the "Create new user" form where you need to fill in:
First Name
Last Name
Year of Birth
After filling out your details, click the Create button to save your profile.
Search For Players:
For existing players, you can search for your profile by typing in your name or part of it in the search field.
The application will list found players in a dropdown under "Found Players".
Select your profile from the list and click the Select button to proceed.

##Playing the Game
Once both players are selected (either by creating new profiles or selecting existing ones), you can start playing Tic-Tac-Toe:

##Game Play:
Single Player (AI): You'll play against an AI that uses the Minimax algorithm, which ensures optimal play from the AI, making it quite challenging.
Two Players: You and your friend take turns to place 'X' or 'O' on the board. The first to get three in a row wins.

##Scoreboard
You can select to see scoreboard from the top left corner. It tracks all players wins, losses and draws.

##Features
Player Management: Ability to create new players or select existing ones.
Advanced AI: Utilizes the Minimax algorithm for a challenging single-player experience.
Simple UI: User-friendly interface designed for ease of use.
You can return to the main menu any time from the top left corner.

##Notes
Ensure you have the .NET Framework installed to run this WinForms application.
The game saves player profiles locally, so make sure you have write permissions in the application directory.


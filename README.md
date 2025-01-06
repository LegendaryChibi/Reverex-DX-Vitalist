# Vitalist

## Summary

This is a project containing a custom statemachine base system for a second player for the sequel to the award winning Reverex, Reverex: DX, of which I was the **Programming Lead**. The second player is responsible for playing optional minigames as well as being foreceably prompted to play minigames.

## Basic System

This project containes a custom Minigame state machine that also has different whole games as MinigameStates. These allow for easy transitions in and out of states. Additionally, pop up minigames forcibly prompt the player to play them, taking their selectable controls until they do so. Included are also overridable minigames that are prompted by the first player that the second player must complete before continuing their own game loop. 

## Purpose

This system was created in such a way that allows the number of minigames and pop ups minigames to be infinitley scaled up or scaled back based on the current project scope, allowing both programmers and artists flexibility while they work towards the finished game. The original Reverex won ⭐**3rd Place**⭐ for *Technical Innovation* at the **Level Up Student Showcase 2024** in Toronto.

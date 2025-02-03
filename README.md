# ProtoCHILL - Unity Prototyping Package  
Protochill is a Unity package that allows for rapid prototyping using ChatGPT.

## Setting Up package in Unity
1. Go to **Window > Package Manager**, click on the "+" button on the top left, then chose "install from git URL"
2. Add the [ChatGPT-Wrapper-For-Unity](https://github.com/GraesonB/ChatGPT-Wrapper-For-Unity) from GIT URL : paste the link https://github.com.GraesonB/ChatGPT-Wrapper-For-Unity.git
3. Add the [ProtoChill Package](https://github.com/jouliet/UnityProtoChill) from GIT URL : paste the link https://github.com/jouliet/UnityProtoChill.git
4. Go to **Window > ProtoChill** to start using the tool.

I you don't git, you need to install it to import the packages:
https://git-scm.com/downloads/win

## Using the package to build a game
1. Chat Section : you may enter a message and submit it to GPT for him to create a UML Structure for you future game
2. Diagram Section : the diagram reflects the structure of the scripts of your game. If the "+", is green, the script has been generated. Otherwhise, see 3.
3. Script generation : you can generate scripts by hitting the "+" on a class in the diagram, and then "generate script" to generate it. If you want better script generation, see 4.
4. Better script generation : If you want to describe the specific behavior of a script before generating it, begin by clicking on the script you want to generate on the diagram. It should light up and the chat box should say "Generating script for...". You can now describe the behavior you want before pressing submit.
5. Warning on script generation 
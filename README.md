# Koinonia
Unity-Oriented Package Manager  

[![Join the chat at https://gitter.im/nitreo/Koinonia](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/nitreo/Koinonia?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## What is Koinonia? 

Following the [original meaning](https://en.wikipedia.org/wiki/Koinonia), Koinonia is a package manager designed to work tightly with Unity Engine. 

## Features

* CLI
* Install package along with it's dependencies
* Post Installer Scripts
* Path mapping to map your repository paths to koinonia installation paths (i.e. Plugins, Project Root, etc)

## Motivation

Asset Store is great, but is not designed to distribute code base.  
Installing stuff manually from github is great, but can be automated for the sake of saving precious time.  

## Philosophy

Koinonia is designed to be lightweight, useful and easy.

## How to install ? 

Download Zip or Clone this repo somewhere into your project Assets folder.  
> Make sure Koinonia is located under Editor folder

## How to use ?

Prerequisites:  
Please, follow [this guide](https://github.com/blog/1509-personal-api-tokens) to create personal access token. You will need it for koinonia to communicate with github.

#### Step 1. 
Once Koinonia is installed, in the top menu locate `Packages -> Manager...`
A terminal will open.

#### Step 2.
In command line: `ghtoken xxxxxxxxxxxxxxxxxxxxxxxxx` where `xxxxxxxxxxxxxxxxxxxxxxxxx` is your github personal access token.
Hit enter.

#### Step 3.
As a first example, you can install CSharp 6.0 support. In terminal type in `install nitreo/csharp60`. 

#### Step 3.
As an second example, you can install uFrame/MVVM or uFrame/ECS. In terminal type in `install uFrame/MVVM` or `install uFrame/ECS`

## How to make my repository compatible with Koinonia ?






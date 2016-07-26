# Koinonia
Unity-Oriented Package Manager  

[![Join the chat at https://gitter.im/nitreo/Koinonia](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/nitreo/Koinonia?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)  

![GitHub Logo](https://i.gyazo.com/c5e58d4bb60f25deeb5806aeffb1e993.png)
## WARNING

Koinonia is in pre-alpha state. Expect bugs, breaking changes, etc.  
Don't use in production.
Currently supports only github-hosted repositories.

Current goals:
* Introduce IoC and Decouple stuff
* Implement Update
* Introduce GUI

## What is Koinonia? 

Following the [original meaning](https://en.wikipedia.org/wiki/Koinonia), Koinonia is a package manager designed to work tightly with Unity Engine. 

## Features

* CLI
* Install package along with it's dependencies by commit, tag or release
* Post Installer Scripts
* Path mapping to map your repository paths to unity special folders (i.e. Plugins, Project Root, etc)
* Works outside of main thread

## Motivation

Asset Store is great, but is not designed to distribute code base.  
Installing stuff manually from github is great, but can be automated for the sake of saving precious time.  

## Philosophy

Koinonia is designed to be lightweight, useful and easy.

## Quick Getting Started

#### Prerequisites  
Please, follow [this guide](https://github.com/blog/1509-personal-api-tokens) to create personal access token. You will need it for koinonia to communicate with github.

#### Step 0.
Download Zip or Clone this repo somewhere into your project Assets folder.  
> Make sure Koinonia is located under Editor folder

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

#### Further information

By default `install author/repo` installs the latest release. If no releases found, it installs the latest tag. If no tags found, it installs the fisrt branch returned by github api.

To specify exact tag/branch/release, you can spice your command up with that info: `install author/repo@branch_or_tag_name`. Make sure that target branch or tag of the repository contains the mandatory `koinonia.config.json` file in the root.

## How to make my repository compatible with Koinonia ?
#### Package Identification
Use the following JSON template to craft a configuration for your package and place it inside `koinonia.config.json` in your repository root:  
```
{
  "Title" : "Package Title",
  "Author" : "Package Author",
  "License" : "Some License (ex. MIT)",
  "RequiresFullReimport" : true,  
  "Mappings" : {
    "Default" : "A/B/C",
    "Root" : "A/B/C"
  },
  "Dependencies" : {
     "repo_owner/repo_name" : "tag_or_branch_name",
     "nitreo/UniRx" : "master"
  }
}
```

##### Title
[STRING, REQUIRED]
##### Author
[STRING, REQUIRED]
##### License
[STRING, REQUIRED]
##### RequiresFullReimport
[BOOLEAN, OPTIONAL] If set to true, will prompt user to reimport all assets when your package is installed]
##### Mappings
[OBJECT, OPTIONAL, PROPERTY SCHEME: "MappingId" : "RelativePathInYourRepository" ]  
If not provided, your entire repo will be installed into `Plugins/ManagedPackages/author/repository/`
###### Default
[STRING] Relative path in your repository that will be copied to `Plugins/ManagedPackages/author/repository/`
###### Root
[STRING,OPTIONAL] Relative path in your repository that will be copied to Project root (no nesting is done as `author/repository`
##### Dependencies
[OBJECT, OPTIONAL, PROPERTY SCHEME: "Repo_Owner/Repo_Name" : "Branch_Or_Tag_Name" ]
  
#### Further information

By default `install author/repo` installs the latest release. If no releases found, it installs the latest tag. If no tags found, it installs the fisrt branch returned by github api.

If you expect your user to install the package using shortcut: `install author/repo`, then make sure to create a release on github, so that koinonia automatically finds it for the user. Otherwise, user will have to specify compatible tag or branch: `install author/repo/master` 
 
  
> Regardless of any mappings, koinonia.config.json and optional installer will be copied inside `Plugins/ManagedPackages/author/repository/`  


## Repos compatible with Koinonia

* https://github.com/uFrame/Core
* https://github.com/uFrame/MVVM
* https://github.com/uFrame/ECS

* https://github.com/nitreo/UniRx
* https://github.com/nitreo/csharp60
* https://github.com/nitreo/unity-rect-extensions
* https://github.com/nitreo/unity-kernel



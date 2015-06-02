# SEBlockControl
This project strives to make development with the programmable block easier and more realistic. In essence SE implements a simulation that controls virtual hardware much in the same manner as many commercial hardware automation tool-sets (minus the communication headaches) do. By applying best practices to code development and testing it is hoped that programmable block development can be faster, easier and have more flexible/repeatable results. By using test and mocking frameworks it is possible to make grids with any imaginable configuration and test against their data/responses without ever loading the game runtime. Assert code can be used to ensure that block automation code runs as expected before ever pasting into the game.

Current Features: 
- Simple Airlock control programmable block - can leverage a room of unlimited size with as many doors, vents and other controls as desired. Supports scrolling LCD status input
- Initial setup/testing of using testing frameworks to exercise programmable block code (currently using MSTest)

# Development Tools
.NET code editors, compilers and testers are available for Windows, OSX and Linux systems at the [Visual Studio](https://www.visualstudio.com/) home page and the [MonoDevelop)[http://www.monodevelop.com] home page.

Free editions of .NET IDEs:
- For a desktop experience on Windows try [Visual Studio Community Edition](https://www.visualstudio.com/vs-2015-product-editions)
- For a desktop experience on Windows, Linux and OSX try [MonoDevelop](http://www.monodevelop.com/download/)
- For a lightweight experience on any platform try [Code](https://www.visualstudio.com/products/code-vs)
- For a web based experience on any platform try [Visual Studio Online](https://www.visualstudio.com/products/what-is-visual-studio-online-vs)

# [SpaceEngineers](http://www.spaceengineersgame.com/)

An open world game that provides a series of complex block elements that allow he player to construct working systems for craft in multiple environments (primarily space). Of the blocks available the most functional and powerful block in the entire game is the [Programmable Block](http://steamcommunity.com/sharedfiles/filedetails/?id=360966557&insideModal=1).

However this block is a challenge to most players, even developers, partially due to language choices but mainly because of a reality of modern software development in that complex systems are not easy to write even abstractly and giving control over a complex system means exposing what is complex about the system or limiting the user by cutting off features in a drive for simplicity/accessibility. 

To lower the bar for accessibility to the programmable block is necessary for it to be leveraged fully in the game. However it may be most prudent to use the workshop features more and leverage public tooling more to support the development of programmable blocks. This is desirable mainly because it is impossible for game developers to achieve even a 5% of what the wider industry is doing with code development nor should they try, they should be focused on great games.

I view open programmable blocks as a critical component (as both a developer and an automation engineer) to the role play experience for the game. They are the bits of code many engineers keep in their pocket for deployment at the right time and place. The thing is they are almost never developed entirely at the time and place they are used and very rarely, even in the real world, use the same tools/systems to run as they did to develop. I strongly advocate an ecosystem of tools and better support for getting code in/out of blocks from popular tools along with an expanded workshop/standards to allow for easy integration of multiple code blocks by different authors. By including the code block Keen has created one of the closest to reality games when it comes to automation. While that choice may come with some hard to use components, the power afforded is unmatched in almost any game without mods added in. 


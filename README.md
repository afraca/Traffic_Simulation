# Traffic Simulation #

**DISCLAIMER: This is (currently) by no means an application you should or can use for anything. Please continue reading for further explanation. This is mainly for educational purposes.**

This project has been built by Stefan Ottevanger and me (Sije Harkema) for an undergraduate computer science course at the Utrecht University (Netherlands) called Modeling and System development (dutch: modelleren en systeemontwikkeling). The course focuses on application development, or to be precise, as Dr. Atze Dijkstra puts it:

> *In short, the purpose of this course is to acquire the knowledge and skills to analyse larger problems, design solutions, and program the solutions using good OO design principles and design patterns.*

##So what's all this code about?##

> *The lectures provide most of the theory, to be applied during the lab assignment. There is only one lab assignment, which can be done in stages, for each stage work has to be submitted (and is judged as a single lab exercise). The lab assignment involves initial requirements analysis, is large enough to be confronted with the necessity of the use of OO design principles and patterns.*

(A copy of the lab assignment in all of its glory is provided in portable document format)
So, this is what we've built in several weeks. It's far from perfect, and that's something I can't stress enough. The project was built in stages, and some things were rushed, therefore partially implemented, and not smoothed as it was of no concern for later stages. 

##Known Issues##
A small summary of known issues: 

* Serialization: This was introduced to provide some basic saving/resuming of simulation to file, and reading and writing maps to file. This was of low priority, and because of no former experience with serialization a rather stone-age implementation. Saving a simulation to disk works, 
* Parameters: When running the application, parameters can be used to tweak around, though some combinations of parameters may due processor load or overflow exceptions.
* Stopwatch: a non-standard clocked cpu may cause the C# stopwatch to misbehave.

##Why publishing?##

So why would we even bother publishing this on Github? Well, you're probably here because you "believe" in open source, so I won't bother going into details about the philosophical debate. This is mainly for **educational purposes**. This is published under the General Public License v3, of which a text copy is provided.

##Where will this go?##

After setting up this repository I expect no real sudden progress will be made. You can file issues if you want, and it might help they're grouped when one might fix some. Pull request will be accepted generously as neither of us are planning on working on this thing anyway. If you have any further questions, use the email address provided by my profile.

##Where is the education?##

As previously mentioned, the lab assignment is in the repository. Our apologies for the lack of full documentation, this can't be provided as it's mainly written in Dutch. An educational class diagram can be found in the class_diagram.pdf, though minor outdated because of the latest refactoring.

##Small tech notes##

This has originally been built with Visual Studio 2010 (Ultimate), and has been adjusted for publication with Visual Studio 2012 Ultimate RC with Resharper 7 EAP. Sorry I didn't bother cleaning up the repo.
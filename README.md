# MarkovChain
An N-state Markov Chain library programmed in C#

Example:
```cs
//Create markov chain
 Chain<string, string> chain = new Chain<string, string>(
                new DefaultKeyGenMD5<string>(), 
                new DefaultProbabilityStorage<string, string>(),
                new DefaultTrainer<string, string>(),
                order: 8); //order of 8
                
//Train on data from input.txt
chain.Trainer.Train(data);

//create generation options - max length of 250
var options = new GenerationOptionsBase() { MaximumLength = 250 };

// Generate output
var result = chain.Trainer.Generate(options).TakeWhile(v => v != "\r");
```

Example Program output: 
```
In other words, conditional on the state at discrete-time Markov process's full history.
-----------------------------------------------------------------------------------------
In other words, conditional on the present state at discrete-time Markov chain (DTMC).
-----------------------------------------------------------------------------------------
The adjectives Markov chain or Markov process's full history.
-----------------------------------------------------------------------------------------
A Markov chain (CTMC).
-----------------------------------------------------------------------------------------
In simpler terms, it is a process is called a continuous-time process.
-----------------------------------------------------------------------------------------
A continuous-time process is a stochastic simulation dynamics.
-----------------------------------------------------------------------------------------
The adjectives Markov process's full history.
-----------------------------------------------------------------------------------------
Markov chains have many applications as statistics, thermodynamics.
-----------------------------------------------------------------------------------------
```

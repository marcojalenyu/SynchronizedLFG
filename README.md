# SynchronizedLFG

This is a simple program that manages the LFG (Looking for Group) dungeon queuing of a hypothetical MMORPG.

## Configuration

Things to consider:
1. There are only n instances that can be concurrently active. Thus, there can only be a maximum n number of parties that are currently in a dungeon.
2. A standard party of 5 is 1 tank, 1 healer, 3 DPS.
3. It is expected to not encounter any deadlock or starvation.
4. Inputs are assumed to arrive at the same time.
5. The clear time of a dungeon is randomly set between t1 and t2.

The following configurations can be set in the 'config.txt' file located in the same folder with the following format:
- n [maximum number of concurrent instances]
- t [number of tank players in the queue]
- h [number of healer players in the queue]
- d [number of DPS players in the queue]
- t1 [minimum time before an instance is finished]
- t2 [maximum time before an instance is finished]

Note: All values must be a non-negative integer (uint) (while n > 0 and 0 < t1 <= t2 <= 15).

Example 'config.txt':
```
n 3
t 10
h 10
d 10
t1 5
t2 15
```

## Input Error Handling

- The ordering of the configurations can be in any order; however, duplicate configurations will be ignored.
- Any invalid lines (either empty, incorrect format, invalid value, or non-existent configurations) will be ignored.
- For any invalid configurations, the program will set a default value for the invalid configuration.
- For time, t1 or t2 = 0 will be set to 1; t1 > t2 will be set to t1 = t2; t2 > 15 will be set to t2 = 15.

## Running the program

1. To run the program, ensure that the 'config.txt' file is in the same directory as the source files.
2. Ensure that the 'config.txt' file is correctly formatted.
3. Run 'Program.cs' on a C# compiler (recommended: Visual Studio).

The program has the following functionalities to handle concurrency and synchronization:
- Threads are used to dispatch players to the dungeon instance and track the statuses of the dungeon instances.
- Locks are used to ensure thread safety (to avoid deadlocks) when accessing shared resources like the party queue and the list of dungeon instances.
- Starvation is generally avoided by looping from the index of the last instance that a party has been assigned to.
- AutoResetEvent is used to signal the start and completion of a run in a dungeon instance, allowing for a print of the status of all dungeon instances.

Note: Using a really high amount of instances (n) may cause the console to be flooded with messages and take a while to complete.
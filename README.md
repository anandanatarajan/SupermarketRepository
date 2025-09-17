# SupermarketRepository

A small dll depending on NPOCO to make the crud related activities much easier,
since the database is defined in the program.cs /startup.cs it can verywell work with all the available drivers for NPOCO like SqlServer/ MySQL/MariaDB/NPGSQL 
view the homecontroller.cs for sample handling

updated 11:27 17-07-2024
added async properties and updated to new class.

updated 12:19 26-07-2024
execute scalar function is updated to accomodate objects

updated 13:13 01-08-2024
modified addnew and addnewasync to return object instead of integer 
possible usecase if primarykey is string it will return that object
in options added loggin so it can injected directly in startup or program.cs
an example is uploaded to git for most of the features

updated 14:45 02-08-2024
changed .net 6.0 to .net 8.0 LTS and npoco to 6.0.1
added mapper functionality to map one object to another object of different type
added constructor injection for testing


### experimental features
*************************************************************
added a custom attribute
SuperAutoIncrementAttribute 
this will enable autoincrement of a non identity field.

you can enable this by making the bool value true for property UseExperimentalFeature which is false by default
proceed with caution at your own risk
**********************************************************************************
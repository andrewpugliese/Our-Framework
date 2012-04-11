/*
Command file for Building the Base One Sample Database

Files NEED to be listed in dependancy order.

Objects which are dependant on other objects need to be
listed after the object they are dependant on.

Files NEED to be listed in relative path from where
the setup application has defined in the config key: DDLSourceDirectory

The following are command keywords:

BreakWithMsg "Your Message Here"
	The setup utility with display your message and wait for a response
	to either quit or continue


RunCmdFile Scripts\B1.Catalog.cmd
	The setup utility will open up and process the contents of the 
	listed command file before continuing with the current command file.


ServerOnly 	SqlDDLFile.sql
	The setup utility will NOT connect to a database but only login
	to the server to process the file. This is used for commands
	that do not require connecting to a particular database such as
	when creating a new database.
    
Field substitutions enclosed with {}
By enclosing a string in the curly braces, the setup utility will replace
the string with the value found in the app.config file.
This allows for each developer to specify different files for their environment
while keeping the main cmd file the same (and only having 1 copy to maintain).

This is helpful when building databases that contain file locations that are
specific to each environment.

 */
 
 
 
// Reset the database
// NOTE: the string ResetDb is a key in the app.config
//      file, which contain the actual file name used.
// To create a database use:
// ServerOnly Scripts\setup\B1.SampleDb.sql
// To only drop user objects, use 
// Scripts\setup\B1.SampleDb.DropObjects.sql
{ResetDb}

// create the table for generating unique ids and sequence numbers
Tables\B1.UniqueIds.sql

// create the tables for configuration
Tables\B1.AppConfigSettings.sql
Tables\B1.AppConfigParameters.sql

// table for testing and demo
Tables\B1.TestSequence.sql
Tables\B1.DbSequenceIds.sql

// tables for User and Application Data
Tables\B1.AccessControlGroups.sql
Tables\B1.UserMaster.sql
Tables\B1.UIControls.sql
Tables\B1.AccessControlGroupRules.sql
Tables\B1.AppMaster.sql
Tables\B1.SignonControl.sql
Tables\B1.UserSessions.sql
Tables\B1.AppSessions.sql

// tables for task processing
Tables\B1.TaskConfigurations.sql
Tables\B1.TaskRegistrations.sql
Tables\B1.TaskStatusCodes.sql
Tables\B1.TaskProcessingQueue.sql
Tables\B1.TaskDependencies.sql

//
// create the stored procedures
//
// create the stored procedures for generating unique ids
Procedures\B1.usp_UniqueIdsGetNextBlock.sql


// create the stored procedures for loading application data dictionary cache
Procedures\B1.usp_CatalogGetColumns.sql
Procedures\B1.usp_CatalogGetPrimaryKeys.sql
Procedures\B1.usp_CatalogGetIndexes.sql
Procedures\B1.usp_CatalogGetFKeys.sql


// reconcile the database catalog with the application catalog
Scripts\setup\B1.LoadInitialData.sql
Scripts\setup\B1.LoadSampleData.sql


// create Family View Objects
Tables\FamilyView\FV.ContentTypes.sql
Tables\FamilyView\FV.ContactTypes.sql
Tables\FamilyView\FV.ViewBooks.sql
Tables\FamilyView\FV.ContentIds.sql
Tables\FamilyView\FV.ContentTags.sql
Tables\FamilyView\FV.Families.sql
Tables\FamilyView\FV.People.sql
Tables\FamilyView\FV.FamilyMembers.sql
Tables\FamilyView\FV.Institutions.sql
Tables\FamilyView\FV.InstitutionMembers.sql
Tables\FamilyView\FV.Events.sql
Tables\FamilyView\FV.EventAttendees.sql
Tables\FamilyView\FV.ViewBookUsers.sql
Tables\FamilyView\FV.ContentTagging.sql
Tables\FamilyView\FV.ContactInformation.sql
Tables\FamilyView\FV.Referrals.sql
Tables\FamilyView\FV.Addresses.sql
Tables\FamilyView\FV.PersonToPerson.sql

Scripts\setup\FV.LoadInitialData.sql
Scripts\setup\FV.LoadSampleData.sql


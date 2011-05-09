--#SET TERMINATOR /
--
--
CREATE TABLE B1.ConfigSettings(
	ConfigSetName varchar(48) NOT NULL,
	Version varchar(48) NOT NULL,
	ConfigXML clob NOT NULL,
  	ConfigSetNameUPPER generated always as (Upper(ConfigSetName)), -- used for case insensitive uniqueness
	VersionUPPER generated always as (Upper(Version)), -- used for case insensitive uniqueness
	PublishedDateTime date NULL,
	EffectiveDateTime date NULL,
	PublishedByUserCode int NULL,
	Description varchar(255) NULL,
 CONSTRAINT PK_ConfigSettings PRIMARY KEY(ConfigSetName, Version)
) in B1Core index in B1CoreIdx
/
CREATE UNIQUE  INDEX B1.UX_ConfigSettings ON B1.ConfigSettings
(ConfigSetNameUPPER, VersionUPPER )
/
GRANT REFERENCES ON B1.ConfigSettings TO PUBLIC
/
GRANT ALL ON B1.ConfigSettings TO PUBLIC 
/

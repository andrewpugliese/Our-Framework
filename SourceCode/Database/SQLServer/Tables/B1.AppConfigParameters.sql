CREATE TABLE B1.AppConfigParameters(
  ParameterName NVARCHAR(48) not null,
  ParameterValue NVARCHAR(256) not null,
 CONSTRAINT AppConfigParameters_PK PRIMARY KEY(ParameterName)
) ON B1Core
GO

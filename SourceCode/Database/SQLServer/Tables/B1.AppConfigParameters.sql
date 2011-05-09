CREATE TABLE B1.AppConfigParameters(
  ParameterName Varchar(48) not null,
  ParameterValue Varchar(256) not null,
 CONSTRAINT PK_AppConfigParameters PRIMARY KEY(ParameterName)
) ON B1Core
go
grant references on B1.AppConfigParameters to public
go
grant insert, select, update, delete on B1.AppConfigParameters to public
go
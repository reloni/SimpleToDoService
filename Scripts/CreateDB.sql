CREATE TABLE TaskUser (
UUID UUID PRIMARY KEY,
FirebaseId varchar(50) NOT NULL UNIQUE,
CreationDate timestamptz NOT NULL,
FirstName varchar(255),
LastName varchar(255),
Email varchar(255) NOT NULL
);

CREATE INDEX FirebaseId_Email_Index ON TaskUser (FirebaseId, Email);

CREATE TABLE Task (
UUID UUID PRIMARY KEY,
Completed BOOL NOT NULL,
Description varchar(255) NOT NULL,
Notes varchar(4000),
CreationDate timestamptz NOT NULL,
TargetDate timestamptz,
TargetDateIncludeTime BOOL NOT NULL,
Owner UUID NOT NULL REFERENCES TaskUser (UUID) ON DELETE CASCADE
);

CREATE TABLE PushNotification
(
UUID UUID PRIMARY KEY,
ServiceId UUID NOT NULL,
TaskId UUID NOT NULL REFERENCES Task (UUID) ON DELETE CASCADE,
UserId UUID NOT NULL REFERENCES TaskUser (UUID) ON DELETE CASCADE,
DueDate timestamptz NOT NULL
);

CREATE INDEX PushNotification_ServiceId ON PushNotification (ServiceId);

update dbversion
set version = 1
where version = 0;

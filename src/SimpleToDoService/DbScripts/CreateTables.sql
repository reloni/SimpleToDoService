CREATE TABLE ToDoUser (
UUID UUID PRIMARY KEY,
FirstName varchar(255) NOT NULL,
LastName varchar(255) NOT NULL,
Email varchar(255) NOT NULL,
Password varchar(255) NOT NULL
);

CREATE TABLE ToDoEntry (
UUID UUID PRIMARY KEY,
Completed BOOL NOT NULL,
Description varchar(255) NOT NULL,
Notes varchar(4000),
CreationDate timestamptz NOT NULL,
TargetDate timestamptz,
Owner UUID REFERENCES ToDoUser (UUID)
);

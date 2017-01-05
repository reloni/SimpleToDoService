CREATE TABLE ToDoUser (
Id serial PRIMARY KEY,
FirstName varchar(255) NOT NULL,
LastName varchar(255) NOT NULL,
Email varchar(255) NOT NULL,
Password varchar(255) NOT NULL
);

CREATE TABLE ToDoEntry (
Id serial PRIMARY KEY,
Completed BOOL NOT NULL,
Description varchar(255) NOT NULL,
Notes varchar(4000),
CreationDate timestamptz NOT NULL,
TargetDate timestamptz,
Owner serial REFERENCES ToDoUser (id)
);

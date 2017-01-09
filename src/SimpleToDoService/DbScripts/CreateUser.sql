CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

insert into taskuser (UUID, CreationDate, FirstName, LastName, email, password)
values (uuid_generate_v4(), current_timestamp, 'John', 'Doe', 'john@domain.com', 'ololo');

insert into taskuser (UUID, CreationDate, FirstName, LastName, email, password)
values (uuid_generate_v4(), current_timestamp, 'Jane', 'Doe', 'jane@domain.com', 'ololo')

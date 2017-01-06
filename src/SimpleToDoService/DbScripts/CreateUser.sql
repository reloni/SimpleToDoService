CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

insert into taskuser (UUID, FirstName, LastName, email, password)
values (uuid_generate_v4(), 'John', 'Doe', 'john@domain.com', 'ololo');

insert into taskuser (UUID, FirstName, LastName, email, password)
values (uuid_generate_v4(), 'Jane', 'Doe', 'jane@domain.com', 'ololo')

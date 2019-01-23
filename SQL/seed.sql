-- drop database payment_gateway;
-- create database payment_gateway;

-- admin password: GRAFT_admin1
-- other passwords: _Graft_1

use pg_prod;

INSERT INTO `AspNetRoles` VALUES 
('853c0619-b051-46b6-817d-44dd0b0e7658','Merchant','MERCHANT','388f0973-5357-4235-ada3-f4648f7dd0d9'),
('99599ece-7f41-40c8-8c48-f1fa045ad8fb','ServiceProvider','SERVICEPROVIDER','b73f2818-07e7-408f-82a6-5b443d4e75b8'),
('a600bf31-b1a4-4ae3-bd24-5b6b8ba60059','Admin','ADMIN','ac94db41-8fc0-4232-8feb-65a4e480dc85')
;

INSERT INTO `AspNetUsers` VALUES 
('db1b2f70-4d1f-4f9b-9973-1e52137d0743','admin','ADMIN','admin','ADMIN',_binary '','AQAAAAEAACcQAAAAEA2XBICm/XF2tuBJhw04lkJB7U2vxGC74sqU769yhurT/5DziplVDNSxlETr2JooNg==','KIWH3KY6YUEY4EYIKYZJQGNONRLQDUBD','3d048106-d763-45cc-afa9-4644c01a3989',NULL,_binary '\0',_binary '\0',NULL,_binary '',0,'a600bf31-b1a4-4ae3-bd24-5b6b8ba60059')
,('dd3e08bd-5180-446f-aa3e-027ff55eed49','merchant1','MERCHANT1@GMAIL_.COM','merchant1@gmail_.com','MERCHANT1@GMAIL_.COM',_binary '','AQAAAAEAACcQAAAAEDUm2Nz8IT5WD/Rg5sj5mvSyQ0plEMVriZcTfOlB1phZMlDbGUywA7ctgMClg1lknw==','LJFDJQBU6K6BLFRPJV5URVQTTIQW2WHW','792ea59d-4ce1-46ae-914a-b8fa7ec196d6',NULL,_binary '\0',_binary '\0',NULL,_binary '',0,'853c0619-b051-46b6-817d-44dd0b0e7658')
,('A4C3E68C-D774-4F8A-91B5-CB408C450534','merchant2','MERCHANT2@GMAIL_.COM','merchant2@gmail_.com','MERCHANT2@GMAIL_.COM',_binary '','AQAAAAEAACcQAAAAEDUm2Nz8IT5WD/Rg5sj5mvSyQ0plEMVriZcTfOlB1phZMlDbGUywA7ctgMClg1lknw==','LJFDJQBU6K6BLFRPJV5URVQTTIQW2WHW','792ea59d-4ce1-46ae-914a-b8fa7ec196d6',NULL,_binary '\0',_binary '\0',NULL,_binary '',0,'853c0619-b051-46b6-817d-44dd0b0e7658')
,('76976E33-6621-4B31-870D-4F2C85EF5DD3','provider1','PROVIDER1@GMAIL_.COM','provider1@gmail_.com','PROVIDER1@GMAIL_.COM',_binary '','AQAAAAEAACcQAAAAEDUm2Nz8IT5WD/Rg5sj5mvSyQ0plEMVriZcTfOlB1phZMlDbGUywA7ctgMClg1lknw==','LJFDJQBU6K6BLFRPJV5URVQTTIQW2WHW','792ea59d-4ce1-46ae-914a-b8fa7ec196d6',NULL,_binary '\0',_binary '\0',NULL,_binary '',0,'99599ece-7f41-40c8-8c48-f1fa045ad8fb')
,('42E2846D-A09E-4AFC-A21D-286802757B39','provider2','PROVIDER2@GMAIL_.COM','provider2@gmail_.com','PROVIDER2@GMAIL_.COM',_binary '','AQAAAAEAACcQAAAAEDUm2Nz8IT5WD/Rg5sj5mvSyQ0plEMVriZcTfOlB1phZMlDbGUywA7ctgMClg1lknw==','LJFDJQBU6K6BLFRPJV5URVQTTIQW2WHW','792ea59d-4ce1-46ae-914a-b8fa7ec196d6',NULL,_binary '\0',_binary '\0',NULL,_binary '',0,'99599ece-7f41-40c8-8c48-f1fa045ad8fb')
;

INSERT INTO `AspNetUserRoles` VALUES 
('db1b2f70-4d1f-4f9b-9973-1e52137d0743','a600bf31-b1a4-4ae3-bd24-5b6b8ba60059'),
('dd3e08bd-5180-446f-aa3e-027ff55eed49','853c0619-b051-46b6-817d-44dd0b0e7658'),
('A4C3E68C-D774-4F8A-91B5-CB408C450534','853c0619-b051-46b6-817d-44dd0b0e7658'),
('76976E33-6621-4B31-870D-4F2C85EF5DD3','99599ece-7f41-40c8-8c48-f1fa045ad8fb'),
('42E2846D-A09E-4AFC-A21D-286802757B39','99599ece-7f41-40c8-8c48-f1fa045ad8fb')
;

INSERT INTO Merchant (UserId, Name, Address, WalletAddress, Status)
VALUES 
('dd3e08bd-5180-446f-aa3e-027ff55eed49','Merchant 1','NY City','G678678678678',0),
('A4C3E68C-D774-4F8A-91B5-CB408C450534','Merchant 2','NY City','G123123123213213',0)
;

INSERT INTO ServiceProvider (UserId, Name, Address, WalletAddress, TransactionFee, Status)
VALUES 
('76976E33-6621-4B31-870D-4F2C85EF5DD3','Provider 1','NY City','Gasdasdasdasdas',0.05,0),
('42E2846D-A09E-4AFC-A21D-286802757B39','Provider 2','NY City','Gertretretretret',0.05,0)
;

INSERT INTO Store (MerchantId, Name, Address, Status)
VALUES 
(1,'Store 1 - Merchant 1','NY City',0),
(1,'Store 2 - Merchant 1','NY City',0),
(2,'Store 1 - Merchant 2','NY City',0),
(2,'Store 2 - Merchant 2','NY City',0)
;

INSERT INTO Terminal (StoreId, MerchantId, ServiceProviderId, SerialNumber, Name, Status)
VALUES 
(1,1,1,'001','Terminal 001',0),
(1,1,1,'002','Terminal 002',0),
(1,1,1,'003','Terminal 003',0),
(1,1,1,'004','Terminal 004',0),
(1,1,1,'005','Terminal 005',0),
(1,1,1,'006','Terminal 006',0),
(1,1,1,'007','Terminal 007',0),
(1,1,1,'008','Terminal 008',0),
(1,1,1,'009','Terminal 009',0),
(1,1,1,'010','Terminal 010',0),
(1,1,1,'011','Terminal 011',0),
(1,1,1,'012','Terminal 012',0),
(1,1,1,'013','Terminal 013',0),
(1,1,1,'014','Terminal 014',0),
(1,1,1,'015','Terminal 015',0),
(1,1,1,'016','Terminal 016',0),
(1,1,1,'017','Terminal 017',0),
(1,1,1,'018','Terminal 018',0),
(1,1,1,'019','Terminal 019',0),
(1,1,1,'020','Terminal 020',0)
;

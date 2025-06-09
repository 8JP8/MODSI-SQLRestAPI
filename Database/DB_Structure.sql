-- --------------------------------------------------------
-- Anfitrião:                    modsi-project.database.windows.net
-- Versão do servidor:           Microsoft SQL Azure (RTM) - 12.0.2000.8
-- SO do servidor:               
-- HeidiSQL Versão:              12.5.0.6677
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES  */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- A despejar estrutura da base de dados para modsi-project
CREATE DATABASE IF NOT EXISTS "modsi-project";
USE "modsi-project";

-- A despejar estrutura para tabela modsi-project.DepartmentKPIs
CREATE TABLE IF NOT EXISTS "DepartmentKPIs" (
	"DepartmentId" INT NOT NULL,
	"KPIId" INT NOT NULL,
	FOREIGN KEY INDEX "FK__Departmen__Depar__308E3499" ("DepartmentId"),
	FOREIGN KEY INDEX "FK__Departmen__KPIId__318258D2" ("KPIId"),
	PRIMARY KEY ("DepartmentId", "KPIId"),
	CONSTRAINT "FK__Departmen__Depar__308E3499" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON UPDATE NO_ACTION ON DELETE CASCADE,
	CONSTRAINT "FK__Departmen__KPIId__318258D2" FOREIGN KEY ("KPIId") REFERENCES "KPIs" ("Id") ON UPDATE NO_ACTION ON DELETE CASCADE
);

-- Exportação de dados não seleccionada.

-- A despejar estrutura para tabela modsi-project.Departments
CREATE TABLE IF NOT EXISTS "Departments" (
	"Id" INT NOT NULL,
	"Name" NVARCHAR(255) NOT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	PRIMARY KEY ("Id")
);

-- Exportação de dados não seleccionada.

-- A despejar estrutura para tabela modsi-project.Groups
CREATE TABLE IF NOT EXISTS "Groups" (
	"Id" INT NOT NULL,
	"Name" NVARCHAR(255) NOT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	PRIMARY KEY ("Id")
);

-- Exportação de dados não seleccionada.

-- A despejar estrutura para tabela modsi-project.KPIs
CREATE TABLE IF NOT EXISTS "KPIs" (
	"Id" INT NOT NULL,
	"Name" NVARCHAR(100) NOT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"Description" NVARCHAR(max) NULL DEFAULT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"Value_1" NVARCHAR(50) NULL DEFAULT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"Value_2" NVARCHAR(50) NULL DEFAULT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"Unit" VARCHAR(50) NULL DEFAULT 'NULL' COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"ByProduct" BIT NOT NULL DEFAULT '(1)',
	PRIMARY KEY ("Id")
);

-- Exportação de dados não seleccionada.

-- A despejar estrutura para tabela modsi-project.Pontos3D
CREATE TABLE IF NOT EXISTS "Pontos3D" (
	"Id" INT NOT NULL,
	"x" FLOAT NOT NULL DEFAULT '(0)',
	"y" FLOAT NOT NULL DEFAULT '(0)',
	"z" FLOAT NOT NULL DEFAULT '(0)',
	PRIMARY KEY ("Id")
);

-- Exportação de dados não seleccionada.

-- A despejar estrutura para tabela modsi-project.RoleDepartmentPermissions
CREATE TABLE IF NOT EXISTS "RoleDepartmentPermissions" (
	"RoleId" INT NOT NULL,
	"DepartmentId" INT NOT NULL,
	"CanRead" BIT NOT NULL DEFAULT '(0)',
	"CanWrite" BIT NOT NULL DEFAULT '(0)',
	FOREIGN KEY INDEX "FK__RoleDepar__Depar__2DB1C7EE" ("DepartmentId"),
	FOREIGN KEY INDEX "FK_RoleDepartmentPermissions_Roles" ("RoleId"),
	PRIMARY KEY ("DepartmentId", "RoleId"),
	CONSTRAINT "FK__RoleDepar__Depar__2DB1C7EE" FOREIGN KEY ("DepartmentId") REFERENCES "Departments" ("Id") ON UPDATE NO_ACTION ON DELETE CASCADE,
	CONSTRAINT "FK_RoleDepartmentPermissions_Roles" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON UPDATE NO_ACTION ON DELETE CASCADE
);

-- Exportação de dados não seleccionada.

-- A despejar estrutura para tabela modsi-project.Roles
CREATE TABLE IF NOT EXISTS "Roles" (
	"Id" INT NOT NULL,
	"Name" NVARCHAR(100) NOT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	PRIMARY KEY ("Id")
);

-- Exportação de dados não seleccionada.

-- A despejar estrutura para tabela modsi-project.Rooms
CREATE TABLE IF NOT EXISTS "Rooms" (
	"Id" NVARCHAR(5) NOT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"JsonData" NVARCHAR(max) NOT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	PRIMARY KEY ("Id")
);

-- Exportação de dados não seleccionada.

-- A despejar estrutura para tabela modsi-project.Users
CREATE TABLE IF NOT EXISTS "Users" (
	"Id" INT NOT NULL,
	"Name" NVARCHAR(255) NOT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"Email" NVARCHAR(255) NOT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"Password" NVARCHAR(255) NOT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"Username" NVARCHAR(50) NOT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"Role" NVARCHAR(20) NULL DEFAULT 'NULL' COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"CreatedAt" DATETIME2(7) NOT NULL,
	"Group" NVARCHAR(20) NULL DEFAULT 'NULL' COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"Salt" NVARCHAR(50) NOT NULL DEFAULT '''''' COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"Tel" NVARCHAR(50) NULL DEFAULT 'NULL' COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"Photo" VARCHAR(300) NULL DEFAULT 'NULL' COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"IsVerified" BIT NOT NULL DEFAULT '(0)',
	PRIMARY KEY ("Id"),
	UNIQUE INDEX "UQ__Users__536C85E47D51AE62" ("Username"),
	UNIQUE INDEX "UQ__Users__A9D10534A950DB34" ("Email")
);

-- Exportação de dados não seleccionada.

-- A despejar estrutura para tabela modsi-project.ValueHistory
CREATE TABLE IF NOT EXISTS "ValueHistory" (
	"Id" INT NOT NULL,
	"KPIId" INT NOT NULL,
	"ChangedByUserId" INT NOT NULL,
	"OldValue_1" VARCHAR(255) NULL DEFAULT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"NewValue_1" VARCHAR(255) NULL DEFAULT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"OldValue_2" VARCHAR(255) NULL DEFAULT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"NewValue_2" VARCHAR(255) NULL DEFAULT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"ChangedAt" DATETIME NULL DEFAULT 'getdate()',
	"Unit" VARCHAR(50) NULL DEFAULT 'NULL' COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"ByProduct" BIT NOT NULL DEFAULT '(1)',
	FOREIGN KEY INDEX "FK__ValueHist__Chang__00AA174D" ("ChangedByUserId"),
	FOREIGN KEY INDEX "FK__ValueHist__KPIId__7FB5F314" ("KPIId"),
	PRIMARY KEY ("Id"),
	CONSTRAINT "FK__ValueHist__Chang__00AA174D" FOREIGN KEY ("ChangedByUserId") REFERENCES "Users" ("Id") ON UPDATE NO_ACTION ON DELETE NO_ACTION,
	CONSTRAINT "FK__ValueHist__KPIId__7FB5F314" FOREIGN KEY ("KPIId") REFERENCES "KPIs" ("Id") ON UPDATE NO_ACTION ON DELETE NO_ACTION
);

-- Exportação de dados não seleccionada.

-- A despejar estrutura para vista modsi-project.database_firewall_rules
-- A criar tabela temporária para vencer erros de dependências VIEW
CREATE TABLE "database_firewall_rules" (
	"id" INT NOT NULL,
	"name" NVARCHAR(128) NOT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"start_ip_address" VARCHAR(45) NOT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"end_ip_address" VARCHAR(45) NOT NULL COLLATE 'SQL_Latin1_General_CP1_CI_AS',
	"create_date" DATETIME NOT NULL,
	"modify_date" DATETIME NOT NULL
) ENGINE=MyISAM;

-- A despejar estrutura para vista modsi-project.database_firewall_rules
-- A remover tabela temporária e a criar estrutura VIEW final
DROP TABLE IF EXISTS "database_firewall_rules";
CREATE VIEW sys.database_firewall_rules AS SELECT id, name, start_ip_address, end_ip_address, create_date, modify_date FROM sys.database_firewall_rules_table;

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

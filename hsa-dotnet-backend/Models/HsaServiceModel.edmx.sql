
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 03/14/2017 20:15:34
-- Generated from EDMX file: C:\Users\pah9qd\Documents\TeamCryptonite\hsa-dotnet-backend\hsa-dotnet-backend\Models\HsaServiceModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [fortresstest];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Account_Account_Id]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Reimbursements] DROP CONSTRAINT [FK_Account_Account_Id];
GO
IF OBJECT_ID(N'[dbo].[FK_LineItem_Product]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[LineItems] DROP CONSTRAINT [FK_LineItem_Product];
GO
IF OBJECT_ID(N'[dbo].[FK_LineItem_Receipt]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[LineItems] DROP CONSTRAINT [FK_LineItem_Receipt];
GO
IF OBJECT_ID(N'[dbo].[FK_Products_Categories]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Products] DROP CONSTRAINT [FK_Products_Categories];
GO
IF OBJECT_ID(N'[dbo].[FK_Receipts_Stores]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Receipts] DROP CONSTRAINT [FK_Receipts_Stores];
GO
IF OBJECT_ID(N'[dbo].[FK_Receipts_Users]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Receipts] DROP CONSTRAINT [FK_Receipts_Users];
GO
IF OBJECT_ID(N'[dbo].[FK_ReimbursementReceipts_Receipt]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ReimbursementReceipts] DROP CONSTRAINT [FK_ReimbursementReceipts_Receipt];
GO
IF OBJECT_ID(N'[dbo].[FK_ReimbursementReceipts_Reimbursement]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ReimbursementReceipts] DROP CONSTRAINT [FK_ReimbursementReceipts_Reimbursement];
GO
IF OBJECT_ID(N'[dbo].[FK_ShoppingListItem_Products]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ShoppingListItems] DROP CONSTRAINT [FK_ShoppingListItem_Products];
GO
IF OBJECT_ID(N'[dbo].[FK_ShoppingListItem_ShoppingLists]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ShoppingListItems] DROP CONSTRAINT [FK_ShoppingListItem_ShoppingLists];
GO
IF OBJECT_ID(N'[dbo].[FK_ShoppingLists_Users]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ShoppingLists] DROP CONSTRAINT [FK_ShoppingLists_Users];
GO
IF OBJECT_ID(N'[dbo].[FK_StoreProducts_Product]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[StoreProducts] DROP CONSTRAINT [FK_StoreProducts_Product];
GO
IF OBJECT_ID(N'[dbo].[FK_StoreProducts_Store]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[StoreProducts] DROP CONSTRAINT [FK_StoreProducts_Store];
GO
IF OBJECT_ID(N'[dbo].[FK_StoreProducts_Stores]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ShoppingListItems] DROP CONSTRAINT [FK_StoreProducts_Stores];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[__RefactorLog]', 'U') IS NOT NULL
    DROP TABLE [dbo].[__RefactorLog];
GO
IF OBJECT_ID(N'[dbo].[Accounts]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Accounts];
GO
IF OBJECT_ID(N'[dbo].[Categories]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Categories];
GO
IF OBJECT_ID(N'[dbo].[LineItems]', 'U') IS NOT NULL
    DROP TABLE [dbo].[LineItems];
GO
IF OBJECT_ID(N'[dbo].[Products]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Products];
GO
IF OBJECT_ID(N'[dbo].[Receipts]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Receipts];
GO
IF OBJECT_ID(N'[dbo].[ReimbursementReceipts]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ReimbursementReceipts];
GO
IF OBJECT_ID(N'[dbo].[Reimbursements]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Reimbursements];
GO
IF OBJECT_ID(N'[dbo].[ShoppingListItems]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ShoppingListItems];
GO
IF OBJECT_ID(N'[dbo].[ShoppingLists]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ShoppingLists];
GO
IF OBJECT_ID(N'[dbo].[StoreProducts]', 'U') IS NOT NULL
    DROP TABLE [dbo].[StoreProducts];
GO
IF OBJECT_ID(N'[dbo].[Stores]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Stores];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'C__RefactorLog'
CREATE TABLE [dbo].[C__RefactorLog] (
    [OperationKey] uniqueidentifier  NOT NULL
);
GO

-- Creating table 'Accounts'
CREATE TABLE [dbo].[Accounts] (
    [AccountId] int IDENTITY(1,1) NOT NULL,
    [UserObjectId] uniqueidentifier  NOT NULL,
    [AccountNum] int  NOT NULL,
    [IsActive] bit  NOT NULL
);
GO

-- Creating table 'Categories'
CREATE TABLE [dbo].[Categories] (
    [CategoryId] int  NOT NULL,
    [Name] varchar(max)  NOT NULL
);
GO

-- Creating table 'LineItems'
CREATE TABLE [dbo].[LineItems] (
    [LineItemId] int IDENTITY(1,1) NOT NULL,
    [ReceiptId] int  NOT NULL,
    [ProductId] int  NOT NULL,
    [Price] decimal(18,2)  NOT NULL,
    [Quantity] int  NOT NULL,
    [IsHsa] bit  NOT NULL
);
GO

-- Creating table 'Products'
CREATE TABLE [dbo].[Products] (
    [ProductId] int IDENTITY(1,1) NOT NULL,
    [Name] varchar(100)  NOT NULL,
    [Description] varchar(max)  NULL,
    [AlwaysHsa] bit  NOT NULL,
    [CategoryId] int  NULL
);
GO

-- Creating table 'Receipts'
CREATE TABLE [dbo].[Receipts] (
    [ReceiptId] int IDENTITY(1,1) NOT NULL,
    [UserObjectId] uniqueidentifier  NULL,
    [DateTime] datetime  NULL,
    [IsScanned] bit  NULL,
    [StoreId] int  NULL,
    [ImageRef] varchar(max)  NULL,
    [OcrRef] varchar(max)  NULL
);
GO

-- Creating table 'Reimbursements'
CREATE TABLE [dbo].[Reimbursements] (
    [ReimbursementId] int IDENTITY(1,1) NOT NULL,
    [AccountId] int  NOT NULL,
    [IsReimbursed] bit  NOT NULL,
    [DateTime] datetime  NOT NULL,
    [Amount] decimal(18,2)  NOT NULL
);
GO

-- Creating table 'ShoppingListItems'
CREATE TABLE [dbo].[ShoppingListItems] (
    [ShoppingListItemId] int IDENTITY(1,1) NOT NULL,
    [ShoppingListId] int  NOT NULL,
    [ProductName] varchar(max)  NULL,
    [ProductId] int  NULL,
    [Quantity] int  NULL,
    [StoreId] int  NULL,
    [Checked] bit  NOT NULL
);
GO

-- Creating table 'ShoppingLists'
CREATE TABLE [dbo].[ShoppingLists] (
    [ShoppingListId] int IDENTITY(1,1) NOT NULL,
    [UserObjectId] uniqueidentifier  NOT NULL,
    [Name] varchar(max)  NULL,
    [Description] varchar(max)  NULL,
    [DateTime] datetime  NULL
);
GO

-- Creating table 'Stores'
CREATE TABLE [dbo].[Stores] (
    [StoreId] int IDENTITY(1,1) NOT NULL,
    [Location] geography  NULL,
    [Name] varchar(50)  NOT NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [UserObjectId] uniqueidentifier  NOT NULL,
    [IsEmployee] bit  NOT NULL,
    [IsActiveUser] bit  NOT NULL,
    [EmailAddress] varchar(max)  NOT NULL,
    [GivenName] varchar(max)  NULL,
    [Surname] varchar(max)  NULL
);
GO

-- Creating table 'ReimbursementReceipts'
CREATE TABLE [dbo].[ReimbursementReceipts] (
    [Receipts_ReceiptId] int  NOT NULL,
    [Reimbursements_ReimbursementId] int  NOT NULL
);
GO

-- Creating table 'StoreProducts'
CREATE TABLE [dbo].[StoreProducts] (
    [Products_ProductId] int  NOT NULL,
    [Stores_StoreId] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [OperationKey] in table 'C__RefactorLog'
ALTER TABLE [dbo].[C__RefactorLog]
ADD CONSTRAINT [PK_C__RefactorLog]
    PRIMARY KEY CLUSTERED ([OperationKey] ASC);
GO

-- Creating primary key on [AccountId] in table 'Accounts'
ALTER TABLE [dbo].[Accounts]
ADD CONSTRAINT [PK_Accounts]
    PRIMARY KEY CLUSTERED ([AccountId] ASC);
GO

-- Creating primary key on [CategoryId] in table 'Categories'
ALTER TABLE [dbo].[Categories]
ADD CONSTRAINT [PK_Categories]
    PRIMARY KEY CLUSTERED ([CategoryId] ASC);
GO

-- Creating primary key on [LineItemId] in table 'LineItems'
ALTER TABLE [dbo].[LineItems]
ADD CONSTRAINT [PK_LineItems]
    PRIMARY KEY CLUSTERED ([LineItemId] ASC);
GO

-- Creating primary key on [ProductId] in table 'Products'
ALTER TABLE [dbo].[Products]
ADD CONSTRAINT [PK_Products]
    PRIMARY KEY CLUSTERED ([ProductId] ASC);
GO

-- Creating primary key on [ReceiptId] in table 'Receipts'
ALTER TABLE [dbo].[Receipts]
ADD CONSTRAINT [PK_Receipts]
    PRIMARY KEY CLUSTERED ([ReceiptId] ASC);
GO

-- Creating primary key on [ReimbursementId] in table 'Reimbursements'
ALTER TABLE [dbo].[Reimbursements]
ADD CONSTRAINT [PK_Reimbursements]
    PRIMARY KEY CLUSTERED ([ReimbursementId] ASC);
GO

-- Creating primary key on [ShoppingListItemId] in table 'ShoppingListItems'
ALTER TABLE [dbo].[ShoppingListItems]
ADD CONSTRAINT [PK_ShoppingListItems]
    PRIMARY KEY CLUSTERED ([ShoppingListItemId] ASC);
GO

-- Creating primary key on [ShoppingListId] in table 'ShoppingLists'
ALTER TABLE [dbo].[ShoppingLists]
ADD CONSTRAINT [PK_ShoppingLists]
    PRIMARY KEY CLUSTERED ([ShoppingListId] ASC);
GO

-- Creating primary key on [StoreId] in table 'Stores'
ALTER TABLE [dbo].[Stores]
ADD CONSTRAINT [PK_Stores]
    PRIMARY KEY CLUSTERED ([StoreId] ASC);
GO

-- Creating primary key on [UserObjectId] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([UserObjectId] ASC);
GO

-- Creating primary key on [Receipts_ReceiptId], [Reimbursements_ReimbursementId] in table 'ReimbursementReceipts'
ALTER TABLE [dbo].[ReimbursementReceipts]
ADD CONSTRAINT [PK_ReimbursementReceipts]
    PRIMARY KEY CLUSTERED ([Receipts_ReceiptId], [Reimbursements_ReimbursementId] ASC);
GO

-- Creating primary key on [Products_ProductId], [Stores_StoreId] in table 'StoreProducts'
ALTER TABLE [dbo].[StoreProducts]
ADD CONSTRAINT [PK_StoreProducts]
    PRIMARY KEY CLUSTERED ([Products_ProductId], [Stores_StoreId] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [AccountId] in table 'Reimbursements'
ALTER TABLE [dbo].[Reimbursements]
ADD CONSTRAINT [FK_Account_Account_Id]
    FOREIGN KEY ([AccountId])
    REFERENCES [dbo].[Accounts]
        ([AccountId])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Account_Account_Id'
CREATE INDEX [IX_FK_Account_Account_Id]
ON [dbo].[Reimbursements]
    ([AccountId]);
GO

-- Creating foreign key on [CategoryId] in table 'Products'
ALTER TABLE [dbo].[Products]
ADD CONSTRAINT [FK_Products_Categories]
    FOREIGN KEY ([CategoryId])
    REFERENCES [dbo].[Categories]
        ([CategoryId])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Products_Categories'
CREATE INDEX [IX_FK_Products_Categories]
ON [dbo].[Products]
    ([CategoryId]);
GO

-- Creating foreign key on [ProductId] in table 'LineItems'
ALTER TABLE [dbo].[LineItems]
ADD CONSTRAINT [FK_LineItem_Product]
    FOREIGN KEY ([ProductId])
    REFERENCES [dbo].[Products]
        ([ProductId])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_LineItem_Product'
CREATE INDEX [IX_FK_LineItem_Product]
ON [dbo].[LineItems]
    ([ProductId]);
GO

-- Creating foreign key on [ReceiptId] in table 'LineItems'
ALTER TABLE [dbo].[LineItems]
ADD CONSTRAINT [FK_LineItem_Receipt]
    FOREIGN KEY ([ReceiptId])
    REFERENCES [dbo].[Receipts]
        ([ReceiptId])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_LineItem_Receipt'
CREATE INDEX [IX_FK_LineItem_Receipt]
ON [dbo].[LineItems]
    ([ReceiptId]);
GO

-- Creating foreign key on [ProductId] in table 'ShoppingListItems'
ALTER TABLE [dbo].[ShoppingListItems]
ADD CONSTRAINT [FK_ShoppingListItem_Products]
    FOREIGN KEY ([ProductId])
    REFERENCES [dbo].[Products]
        ([ProductId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ShoppingListItem_Products'
CREATE INDEX [IX_FK_ShoppingListItem_Products]
ON [dbo].[ShoppingListItems]
    ([ProductId]);
GO

-- Creating foreign key on [StoreId] in table 'Receipts'
ALTER TABLE [dbo].[Receipts]
ADD CONSTRAINT [FK_Receipts_Stores]
    FOREIGN KEY ([StoreId])
    REFERENCES [dbo].[Stores]
        ([StoreId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Receipts_Stores'
CREATE INDEX [IX_FK_Receipts_Stores]
ON [dbo].[Receipts]
    ([StoreId]);
GO

-- Creating foreign key on [UserObjectId] in table 'Receipts'
ALTER TABLE [dbo].[Receipts]
ADD CONSTRAINT [FK_Receipts_Users]
    FOREIGN KEY ([UserObjectId])
    REFERENCES [dbo].[Users]
        ([UserObjectId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Receipts_Users'
CREATE INDEX [IX_FK_Receipts_Users]
ON [dbo].[Receipts]
    ([UserObjectId]);
GO

-- Creating foreign key on [ShoppingListId] in table 'ShoppingListItems'
ALTER TABLE [dbo].[ShoppingListItems]
ADD CONSTRAINT [FK_ShoppingListItem_ShoppingLists]
    FOREIGN KEY ([ShoppingListId])
    REFERENCES [dbo].[ShoppingLists]
        ([ShoppingListId])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ShoppingListItem_ShoppingLists'
CREATE INDEX [IX_FK_ShoppingListItem_ShoppingLists]
ON [dbo].[ShoppingListItems]
    ([ShoppingListId]);
GO

-- Creating foreign key on [StoreId] in table 'ShoppingListItems'
ALTER TABLE [dbo].[ShoppingListItems]
ADD CONSTRAINT [FK_StoreProducts_Stores]
    FOREIGN KEY ([StoreId])
    REFERENCES [dbo].[Stores]
        ([StoreId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_StoreProducts_Stores'
CREATE INDEX [IX_FK_StoreProducts_Stores]
ON [dbo].[ShoppingListItems]
    ([StoreId]);
GO

-- Creating foreign key on [UserObjectId] in table 'ShoppingLists'
ALTER TABLE [dbo].[ShoppingLists]
ADD CONSTRAINT [FK_ShoppingLists_Users]
    FOREIGN KEY ([UserObjectId])
    REFERENCES [dbo].[Users]
        ([UserObjectId])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ShoppingLists_Users'
CREATE INDEX [IX_FK_ShoppingLists_Users]
ON [dbo].[ShoppingLists]
    ([UserObjectId]);
GO

-- Creating foreign key on [Receipts_ReceiptId] in table 'ReimbursementReceipts'
ALTER TABLE [dbo].[ReimbursementReceipts]
ADD CONSTRAINT [FK_ReimbursementReceipts_Receipt]
    FOREIGN KEY ([Receipts_ReceiptId])
    REFERENCES [dbo].[Receipts]
        ([ReceiptId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Reimbursements_ReimbursementId] in table 'ReimbursementReceipts'
ALTER TABLE [dbo].[ReimbursementReceipts]
ADD CONSTRAINT [FK_ReimbursementReceipts_Reimbursement]
    FOREIGN KEY ([Reimbursements_ReimbursementId])
    REFERENCES [dbo].[Reimbursements]
        ([ReimbursementId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ReimbursementReceipts_Reimbursement'
CREATE INDEX [IX_FK_ReimbursementReceipts_Reimbursement]
ON [dbo].[ReimbursementReceipts]
    ([Reimbursements_ReimbursementId]);
GO

-- Creating foreign key on [Products_ProductId] in table 'StoreProducts'
ALTER TABLE [dbo].[StoreProducts]
ADD CONSTRAINT [FK_StoreProducts_Product]
    FOREIGN KEY ([Products_ProductId])
    REFERENCES [dbo].[Products]
        ([ProductId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Stores_StoreId] in table 'StoreProducts'
ALTER TABLE [dbo].[StoreProducts]
ADD CONSTRAINT [FK_StoreProducts_Store]
    FOREIGN KEY ([Stores_StoreId])
    REFERENCES [dbo].[Stores]
        ([StoreId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_StoreProducts_Store'
CREATE INDEX [IX_FK_StoreProducts_Store]
ON [dbo].[StoreProducts]
    ([Stores_StoreId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------
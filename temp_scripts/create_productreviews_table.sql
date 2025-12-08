-- 创建产品评价表
CREATE TABLE IF NOT EXISTS `productreviews` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ProductId` int NOT NULL,
  `UserId` int NOT NULL,
  `Rating` int NOT NULL,
  `Comment` varchar(500) DEFAULT NULL,
  `CreatedAt` datetime NOT NULL,
  `UpdatedAt` datetime DEFAULT NULL,
  `IsDeleted` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`),
  KEY `IX_ProductReviews_ProductId` (`ProductId`),
  KEY `IX_ProductReviews_UserId` (`UserId`),
  CONSTRAINT `FK_ProductReviews_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `products` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_ProductReviews_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
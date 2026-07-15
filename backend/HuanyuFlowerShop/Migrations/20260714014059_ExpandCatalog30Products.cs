using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HuanyuFlowerShop.Migrations
{
    /// <inheritdoc />
    public partial class ExpandCatalog30Products : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var categories = new (string Name, string Description, int SortOrder)[]
            {
                ("绣球", "团簇丰盈，承载圆满与希望", 15),
                ("山茶", "清雅坚定，冬日里的温柔亮色", 16),
                ("桔梗", "轻盈含蓄，象征不变的爱", 17),
                ("蝴蝶兰", "优雅灵动，适合祝福与庆贺", 18),
                ("小苍兰", "清甜芬芳，带来明朗好心情", 19),
                ("风信子", "浪漫丰盈，传递温柔思念", 20),
                ("雏菊", "明媚纯真，收藏日常小确幸", 21),
                ("尤加利", "银绿清新，为空间带来自然呼吸", 22)
            };

            foreach (var category in categories)
            {
                migrationBuilder.Sql($"""
                    INSERT INTO `Categories` (`Name`, `Description`, `SortOrder`, `IsActive`, `CreatedAt`, `UpdatedAt`)
                    SELECT '{Escape(category.Name)}', '{Escape(category.Description)}', {category.SortOrder}, 1, UTC_TIMESTAMP(6), NULL
                    WHERE NOT EXISTS (SELECT 1 FROM `Categories` WHERE `Name` = '{Escape(category.Name)}');
                    """);
            }

            var products = new (string Name, string Description, string Price, string Image, int Stock, bool Featured, string Size, string Material, string Occasion, string Category)[]
            {
                ("晨雾白玫瑰", "象牙白玫瑰与白色洋桔梗层层舒展，搭配银绿尤加利，像清晨薄雾一样安静明亮。适合纪念日、探望与想把温柔说出口的普通一天。", "129.00", "/images/catalog/chenwu-white-rose.png", 68, true, "中型花束", "白玫瑰、白洋桔梗、尤加利", "纪念日、探望、日常赠礼", "玫瑰"),
                ("蜜桃雪山玫瑰", "奶油蜜桃色雪山玫瑰柔和渐变，花瓣丰润有层次。甜而不腻的暖粉色调，很适合生日、约会和送给珍视的人。", "159.00", "/images/catalog/peach-avalanche-rose.png", 52, true, "中大型花束", "蜜桃雪山玫瑰、喷泉草、尤加利", "生日、约会、表白", "玫瑰"),
                ("香槟时光玫瑰", "香槟玫瑰与奶白配花交织出温柔光泽，低调、耐看又有仪式感。送恋人、朋友或用于周年纪念都恰到好处。", "169.00", "/images/catalog/champagne-time-rose.png", 46, true, "中大型花束", "香槟玫瑰、白色配花、尤加利", "周年、生日、祝福", "玫瑰"),
                ("海盐蓝绣球", "雾蓝绣球像一团清凉海风，搭配白色小花和银叶，呈现通透干净的蓝白色系。适合乔迁、生日与夏日问候。", "139.00", "/images/catalog/sea-salt-blue-hydrangea.png", 39, true, "中型花束", "蓝绣球、白色配花、银叶菊", "乔迁、生日、探望", "绣球"),
                ("云朵白绣球", "白色绣球饱满得像云朵，辅以浅粉玫瑰与轻盈绿叶，整体纯净又柔软。适合婚礼祝福、纪念日和雅致家居插花。", "149.00", "/images/catalog/cloud-white-hydrangea.png", 42, false, "中型花束", "白绣球、浅粉玫瑰、绿叶", "婚礼、纪念日、家居", "绣球"),
                ("春日粉郁金香", "粉色郁金香线条利落，花苞轻盈挺拔，像刚刚到来的春天。清甜粉白包装让它适合送恋人、闺蜜或犒赏自己。", "119.00", "/images/catalog/spring-pink-tulip.png", 74, true, "中型花束", "粉郁金香、雪柳、尤加利", "约会、生日、日常悦己", "郁金香"),
                ("黄油郁金香", "柔黄郁金香带着黄油般温暖的光泽，搭配奶白花纸，明快却不张扬。适合庆祝新开始，也适合给朋友送去好心情。", "109.00", "/images/catalog/butter-yellow-tulip.png", 61, false, "中型花束", "黄色郁金香、白色配花", "毕业、开业、朋友赠礼", "郁金香"),
                ("紫夜郁金香", "深紫郁金香自带克制高级感，配以灰粉包装与银绿叶材，神秘而优雅。适合个性告白、艺术空间与纪念日。", "139.00", "/images/catalog/violet-night-tulip.png", 37, false, "中型花束", "紫郁金香、尤加利", "纪念日、告白、艺术赠礼", "郁金香"),
                ("月光白百合", "洁白百合在柔光中舒展，清雅香气与修长花枝让空间立刻明亮起来。适合探望、乔迁、长辈祝福与家居插花。", "149.00", "/images/catalog/moonlight-white-lily.png", 44, true, "大型花束", "白百合、白色配花、尤加利", "探望、乔迁、长辈祝福", "百合"),
                ("粉焰亚洲百合", "粉色亚洲百合花瓣带有细腻渐变，热烈中保留温柔。明亮的色彩很适合生日庆祝、朋友相聚与表达感谢。", "139.00", "/images/catalog/pink-flame-lily.png", 51, false, "大型花束", "粉色亚洲百合、绿叶", "生日、感谢、聚会", "百合"),
                ("橙霞亚洲百合", "橙色百合像落日余晖，花型大方、色彩有活力。搭配奶油色陶瓷花器，适合开业、升职和元气满满的祝福。", "159.00", "/images/catalog/orange-sunset-lily.png", 35, false, "大型花礼", "橙色亚洲百合、季节绿叶", "开业、升职、庆贺", "百合"),
                ("奶油向日葵", "浅奶黄色向日葵与白色小花组成柔和的阳光花束，既有活力又不刺眼。适合毕业、生日和给努力生活的人打气。", "119.00", "/images/catalog/cream-sunflower.png", 63, true, "中型花束", "奶油向日葵、白色小花、尤加利", "毕业、生日、鼓励", "向日葵"),
                ("琥珀向日葵", "金黄向日葵搭配焦糖色配花，呈现温暖浓郁的琥珀色调。是一束适合庆祝、开业和秋日问候的能量花礼。", "129.00", "/images/catalog/amber-sunflower.png", 57, false, "中大型花束", "向日葵、焦糖色配花、绿叶", "开业、庆祝、秋日赠礼", "向日葵"),
                ("柔粉康乃馨", "柔粉康乃馨层层叠叠，配以奶白洋桔梗和轻盈小花，温柔又耐看。适合送妈妈、长辈、老师与表达长久感谢。", "99.00", "/images/catalog/soft-pink-carnation.png", 82, true, "中型花束", "粉康乃馨、白洋桔梗、满天星", "母亲节、感谢、长辈赠礼", "康乃馨"),
                ("奶白康乃馨", "奶白康乃馨搭配香槟色花纸，安静、纯净且富有质感。适合探望、感谢和不需要浓烈语言的温柔陪伴。", "95.00", "/images/catalog/ivory-carnation.png", 76, false, "中型花束", "白康乃馨、尤加利、白色配花", "探望、感谢、陪伴", "康乃馨"),
                ("星河满天星", "白色满天星细密铺开，像落在花束里的星河。轻盈银白包装保留空气感，适合告白、毕业和纪念纯粹心意。", "89.00", "/images/catalog/starlight-babys-breath.png", 88, true, "中型花束", "白色满天星", "告白、毕业、纪念", "满天星"),
                ("雾蓝满天星", "雾蓝满天星与少量白色星点交织，清透而梦幻。适合送给喜欢蓝色的人，也适合桌面装饰和拍照花礼。", "99.00", "/images/catalog/mist-blue-babys-breath.png", 69, false, "中型花束", "雾蓝满天星、白色满天星", "生日、朋友赠礼、桌面装饰", "满天星"),
                ("茉莉清风", "洁白茉莉与细枝绿叶组成小巧清新的香氛花礼，气息自然不甜腻。适合书桌、卧室与探望时表达安静关怀。", "79.00", "/images/catalog/jasmine-breeze.png", 48, false, "小型花礼", "茉莉、季节绿叶", "家居、探望、日常悦己", "茉莉"),
                ("桂花金雨", "细小金桂缀满枝头，像一阵带香气的金色细雨。搭配素雅陶瓶，适合中秋问候、长辈赠礼与东方家居陈设。", "129.00", "/images/catalog/osmanthus-golden-rain.png", 33, false, "中型瓶花", "金桂、季节枝叶", "中秋、长辈赠礼、家居", "桂花"),
                ("山茶绯雪", "绯粉山茶与白色花材交错盛放，花瓣精致、色彩清亮。既有东方韵味也有现代感，适合新年、生日和雅致祝福。", "179.00", "/images/catalog/camellia-snow.png", 29, true, "中大型花礼", "粉山茶、白色配花、绿叶", "新年、生日、雅致赠礼", "山茶"),
                ("桔梗晚风", "蓝紫桔梗带着晚风般的清凉气质，搭配灰绿叶材与雾白包装。适合送朋友、老师和喜欢安静色彩的人。", "109.00", "/images/catalog/balloon-flower-evening.png", 54, false, "中型花束", "蓝紫桔梗、尤加利", "朋友赠礼、教师节、生日", "桔梗"),
                ("洋桔梗奶油杯", "奶油白与浅粉洋桔梗柔软蓬松，盛在简洁奶白花器中，像一杯轻甜云朵。适合乔迁、生日与家居摆台。", "149.00", "/images/catalog/cream-lisianthus.png", 47, true, "中型瓶花", "奶油洋桔梗、浅粉洋桔梗", "乔迁、生日、家居", "桔梗"),
                ("白色蝴蝶兰", "白色蝴蝶兰花姿舒展，枝条利落优雅，配现代感陶盆。适合开业、乔迁、商务祝福与长期室内观赏。", "299.00", "/images/catalog/white-orchid.png", 24, true, "大型盆花", "白色蝴蝶兰、苔藓、陶盆", "开业、乔迁、商务赠礼", "蝴蝶兰"),
                ("胭脂牡丹", "胭脂粉牡丹花型饱满华丽，搭配淡粉与奶白配花，层次丰富却不过分浓艳。适合重要生日、周年与隆重祝福。", "269.00", "/images/catalog/rouge-peony.png", 26, true, "大型花束", "粉牡丹、浅粉配花、尤加利", "重要生日、周年、隆重祝福", "牡丹"),
                ("荷塘月色", "粉白荷花与莲蓬、阔叶构成一幅清润东方花景，适合放在客厅或茶席。送长辈、乔迁和夏日雅集都很特别。", "199.00", "/images/catalog/lotus-moonlight.png", 31, false, "大型瓶花", "荷花、莲蓬、荷叶", "长辈赠礼、乔迁、茶席", "荷花"),
                ("小苍兰晨光", "白黄小苍兰轻盈成簇，清甜香气像晨光一样明快。淡黄与粉白包装让它很适合生日、毕业和日常好心情。", "109.00", "/images/catalog/freesia-morning.png", 58, false, "中型花束", "小苍兰、白色配花、绿叶", "生日、毕业、日常赠礼", "小苍兰"),
                ("风信子浅梦", "粉紫风信子饱满成穗，色彩像轻柔梦境，配以通透玻璃花器。适合桌面、卧室和送给喜欢香气花材的人。", "119.00", "/images/catalog/hyacinth-dream.png", 41, false, "中型瓶花", "粉紫风信子、玻璃花器", "生日、家居、朋友赠礼", "风信子"),
                ("雏菊晴天", "白色雏菊与黄色花心像一张张笑脸，搭配浅蓝与奶白包装，清新有朝气。适合毕业、友情与周末的小惊喜。", "79.00", "/images/catalog/daisy-sunshine.png", 91, true, "中型花束", "白色雏菊、绿叶", "毕业、友情、日常惊喜", "雏菊"),
                ("银叶尤加利花束", "不同形态的银绿尤加利组成极简叶材花束，气味清新、线条松弛。可自然风干，适合家居、摄影与极简审美礼物。", "69.00", "/images/catalog/eucalyptus-silver.png", 73, false, "中型叶材束", "银元尤加利、细叶尤加利", "家居、摄影、日常赠礼", "尤加利"),
                ("四季花园混合花束", "玫瑰、洋桔梗、郁金香与小季节花材组成粉白花园，丰富却有呼吸感。适合生日、周年、求婚和每一个值得庆祝的时刻。", "239.00", "/images/catalog/four-season-garden.png", 36, true, "大型花束", "玫瑰、洋桔梗、郁金香、季节配花", "生日、周年、求婚、庆祝", "组合花束")
            };

            foreach (var product in products)
            {
                migrationBuilder.Sql($"""
                    INSERT INTO `Products` (`Name`, `Description`, `Price`, `ImageUrl`, `Stock`, `IsFeatured`, `IsActive`, `SalesCount`, `Popularity`, `Size`, `Material`, `Occasion`, `CategoryId`, `CreatedAt`, `UpdatedAt`)
                    SELECT '{Escape(product.Name)}', '{Escape(product.Description)}', {product.Price}, '{Escape(product.Image.Replace(".png", ".webp"))}', {product.Stock}, {(product.Featured ? 1 : 0)}, 1, 0, 0, '{Escape(product.Size)}', '{Escape(product.Material)}', '{Escape(product.Occasion)}', c.`Id`, UTC_TIMESTAMP(6), NULL
                    FROM `Categories` c
                    WHERE c.`Name` = '{Escape(product.Category)}'
                      AND NOT EXISTS (SELECT 1 FROM `Products` WHERE `Name` = '{Escape(product.Name)}')
                    LIMIT 1;
                    """);
            }

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var productNames = new[]
            {
                "晨雾白玫瑰", "蜜桃雪山玫瑰", "香槟时光玫瑰", "海盐蓝绣球", "云朵白绣球", "春日粉郁金香", "黄油郁金香", "紫夜郁金香", "月光白百合", "粉焰亚洲百合",
                "橙霞亚洲百合", "奶油向日葵", "琥珀向日葵", "柔粉康乃馨", "奶白康乃馨", "星河满天星", "雾蓝满天星", "茉莉清风", "桂花金雨", "山茶绯雪",
                "桔梗晚风", "洋桔梗奶油杯", "白色蝴蝶兰", "胭脂牡丹", "荷塘月色", "小苍兰晨光", "风信子浅梦", "雏菊晴天", "银叶尤加利花束", "四季花园混合花束"
            };
            var names = string.Join(", ", productNames.Select(name => $"'{Escape(name)}'"));
            migrationBuilder.Sql($"DELETE FROM `Products` WHERE `Name` IN ({names});");

            var categoryNames = new[] { "绣球", "山茶", "桔梗", "蝴蝶兰", "小苍兰", "风信子", "雏菊", "尤加利" };
            var categories = string.Join(", ", categoryNames.Select(name => $"'{Escape(name)}'"));
            migrationBuilder.Sql($"DELETE FROM `Categories` WHERE `Name` IN ({categories}) AND NOT EXISTS (SELECT 1 FROM `Products` WHERE `Products`.`CategoryId` = `Categories`.`Id`);");

        }

        private static string Escape(string value) => value.Replace("'", "''");
    }
}

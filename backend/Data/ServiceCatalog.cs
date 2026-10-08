using LuxeBeautyPlace.Api.Models;

namespace LuxeBeautyPlace.Api.Data;

public static class ServiceCatalog
{
    private const string HairImage = "/service-images/photo_2026-08-20_18-30-46.jpg";
    private const string BraidsImage = "/service-images/photo_2026-08-20_00-29-03 (2).jpg";
    private const string PedicureImage = "/service-images/photo_2026-08-20_23-00-53.jpg";

    public static IReadOnlyList<Service> Services { get; } =
    [
        S(1, "Regular wash", "Hair treatments", "Cleanse and refresh service.", 3000, "NGN 3,000", 45, HairImage),
        S(2, "Deep conditioning wash", "Hair treatments", "Wash with deep conditioning care.", 4000, "NGN 4,000", 60, HairImage),
        S(3, "Antidandruff wash", "Hair treatments", "Wash focused on dandruff control.", 5000, "NGN 5,000", 60, HairImage),
        S(4, "Relaxing", "Hair treatments", "Hair relaxing service.", 5000, "NGN 5,000", 90, HairImage),
        S(5, "Texturizing", "Hair treatments", "Texture softening and styling preparation.", 6000, "NGN 6,000", 90, HairImage),
        S(6, "Deep conditioning", "Hair treatments", "Moisture and strengthening treatment.", 10000, "NGN 10,000", 75, HairImage),
        S(7, "Anti dandruff treatment", "Hair treatments", "Targeted anti dandruff treatment service.", 15000, "NGN 15,000", 90, HairImage),
        S(8, "Regular Cornrows take down", "Take down", "Take down service for regular cornrows.", 1500, "From NGN 1,500", 45, BraidsImage),
        S(9, "Cornrows with extension take down", "Take down", "Take down service for cornrows with extensions.", 4000, "From NGN 4,000", 60, BraidsImage),
        S(10, "Braids take down", "Take down", "Take down service for braids.", 5000, "From NGN 5,000", 90, BraidsImage),
        S(11, "Mini/Micro twists take down", "Take down", "Take down service for mini or micro twists.", 7000, "From NGN 7,000", 120, BraidsImage),
        S(12, "Ponytails take down", "Take down", "Take down service for ponytails.", 3000, "From NGN 3,000", 45, HairImage),
        S(13, "Sew ins take down", "Take down", "Take down service for sew ins.", 3500, "From NGN 3,500", 60, HairImage),
        S(14, "Medium Cornrows (up to 12)", "Braids - No extension", "Medium cornrows without extensions, up to 12 rows.", 3000, "NGN 3,000", 90, BraidsImage),
        S(15, "Small Cornrows (up to 15)", "Braids - No extension", "Small cornrows without extensions, up to 15 rows.", 4000, "NGN 4,000", 120, BraidsImage),
        S(16, "Tiny cornrows", "Braids - No extension", "Tiny cornrows without extensions.", 6000, "From NGN 6,000", 150, BraidsImage),
        S(17, "Smedium braids/twists", "Braids - No extension", "Smedium braids or twists without extensions.", 5000, "NGN 5,000", 150, BraidsImage),
        S(18, "Small braids/twists", "Braids - No extension", "Small braids or twists without extensions.", 9000, "NGN 9,000", 210, BraidsImage),
        S(19, "Mini twists", "Braids - No extension", "Mini twists without extensions.", 15000, "NGN 15,000", 240, BraidsImage),
        S(20, "Micro twists", "Braids - No extension", "Micro twists without extensions.", 20000, "NGN 20,000", 300, BraidsImage),
        S(21, "Knotless braids - Bra length, tiny", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 35000, "NGN 35,000", 360, BraidsImage),
        S(22, "Knotless braids - Bra length, small", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 25000, "NGN 25,000", 330, BraidsImage),
        S(23, "Knotless braids - Bra length, smedium", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 22000, "NGN 22,000", 300, BraidsImage),
        S(24, "Knotless braids - Bra length, medium", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 20000, "NGN 20,000", 270, BraidsImage),
        S(25, "Knotless braids - Waist length, tiny", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 50000, "NGN 50,000", 420, BraidsImage),
        S(26, "Knotless braids - Waist length, small", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 35000, "NGN 35,000", 390, BraidsImage),
        S(27, "Knotless braids - Waist length, smedium", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 28000, "NGN 28,000", 360, BraidsImage),
        S(28, "Knotless braids - Waist length, medium", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 25000, "NGN 25,000", 330, BraidsImage),
        S(29, "Knotless braids - Bum length, tiny", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 65000, "NGN 65,000", 480, BraidsImage),
        S(30, "Knotless braids - Bum length, small", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 40000, "NGN 40,000", 450, BraidsImage),
        S(31, "Knotless braids - Bum length, smedium", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 35000, "NGN 35,000", 420, BraidsImage),
        S(32, "Knotless braids - Bum length, medium", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 32000, "NGN 32,000", 390, BraidsImage),
        S(33, "Knotless braids - Hip length, tiny", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 75000, "NGN 75,000", 540, BraidsImage),
        S(34, "Knotless braids - Hip length, small", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 55000, "NGN 55,000", 510, BraidsImage),
        S(35, "Knotless braids - Hip length, smedium", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 50000, "NGN 50,000", 480, BraidsImage),
        S(36, "Knotless braids - Hip length, medium", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 45000, "NGN 45,000", 450, BraidsImage),
        S(37, "Knotless braids - Knee length, tiny", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 100000, "NGN 100,000", 600, BraidsImage),
        S(38, "Knotless braids - Knee length, small", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 70000, "NGN 70,000", 570, BraidsImage),
        S(39, "Knotless braids - Knee length, smedium", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 60000, "NGN 60,000", 540, BraidsImage),
        S(40, "Knotless braids - Knee length, medium", "Braids with extensions", "Boho braids cost an extra NGN 10,000 per length/size.", 55000, "NGN 55,000", 510, BraidsImage),
        S(41, "Half sew in - tiny", "Cornrows with extensions", "Cornrows with extensions for half sew in.", 40000, "NGN 40,000-NGN 60,000", 300, BraidsImage),
        S(42, "Half sew in - small", "Cornrows with extensions", "Cornrows with extensions for half sew in.", 25000, "NGN 25,000-NGN 40,000", 240, BraidsImage),
        S(43, "Fulani braids - tiny", "Cornrows with extensions", "Fulani braids with extensions.", 40000, "NGN 40,000", 300, BraidsImage),
        S(44, "Fulani braids - small", "Cornrows with extensions", "Fulani braids with extensions.", 30000, "NGN 30,000", 240, BraidsImage),
        S(45, "Small twists", "Natural Twists - with extensions", "Natural twists with extensions.", 20000, "From NGN 20,000", 240, BraidsImage),
        S(46, "SMedium twists", "Natural Twists - with extensions", "Natural twists with extensions.", 18000, "From NGN 18,000", 210, BraidsImage),
        S(47, "Mini Twists", "Natural Twists - with extensions", "Mini twists with extensions.", 25000, "From NGN 25,000", 300, BraidsImage),
        S(48, "Micro twists", "Natural Twists - with extensions", "Micro twists with extensions.", 40000, "From NGN 40,000", 420, BraidsImage),
        S(49, "Passion twists (long)", "Natural Twists - with extensions", "Long passion twists with extensions.", 25000, "NGN 25,000", 240, BraidsImage),
        S(50, "Passion twists (short)", "Natural Twists - with extensions", "Short passion twists with extensions.", 20000, "NGN 20,000", 210, BraidsImage),
        S(51, "Leave out", "Sew ins", "Leave out sew in service.", 20000, "NGN 20,000", 180, HairImage),
        S(52, "Closure", "Sew ins", "Closure sew in service.", 25000, "NGN 25,000", 210, HairImage),
        S(53, "Hybrid (small size)", "Sew ins", "Hybrid sew in with small braids.", 30000, "NGN 30,000", 240, HairImage),
        S(54, "Hybrid (tiny)", "Sew ins", "Hybrid sew in with tiny braids.", 35000, "NGN 35,000", 270, HairImage),
        S(55, "Hybrid (360 small braids)", "Sew ins", "360 hybrid sew in with small braids.", 40000, "NGN 40,000", 300, HairImage),
        S(56, "Hybrid (360 tiny)", "Sew ins", "360 hybrid sew in with tiny braids.", 50000, "NGN 50,000", 360, HairImage),
        S(57, "Regular ponytail (clip in)", "Ponytails", "Regular ponytail using clip in method.", 10000, "NGN 10,000", 90, HairImage),
        S(58, "Regular ponytail (sew in)", "Ponytails", "Regular ponytail using sew in method.", 12000, "NGN 12,000", 120, HairImage),
        S(59, "Regular ponytail with bangs", "Ponytails", "Regular ponytail finished with bangs.", 15000, "NGN 15,000", 150, HairImage),
        S(60, "Half up", "Ponytails", "Half up ponytail style.", 15000, "NGN 15,000", 150, HairImage),
        S(61, "Half up with bangs", "Ponytails", "Half up ponytail style with bangs.", 20000, "NGN 20,000", 180, HairImage),
        S(62, "Individual install", "Crotchet", "Individual crochet install.", 25000, "NGN 25,000", 240, BraidsImage),
        S(63, "Cornrows", "Crotchet", "Crochet cornrows install.", 20000, "NGN 20,000", 180, BraidsImage),
        S(64, "Mixed install", "Crotchet", "Mixed crochet install.", 22000, "NGN 22,000", 210, BraidsImage),
        S(65, "Regular pedicure", "Pedicure", "Classic pedicure service.", 10000, "NGN 10,000", 60, PedicureImage),
        S(66, "Male pedicure", "Pedicure", "Pedicure service for men.", 12000, "NGN 12,000", 60, PedicureImage),
        S(67, "Jelly pedicure", "Pedicure", "Jelly pedicure service.", 15000, "NGN 15,000", 75, PedicureImage),
        S(68, "Closure revamp", "Wigs", "Closure wig revamp service.", 12000, "From NGN 12,000", 90, HairImage),
        S(69, "Frontal revamp", "Wigs", "Frontal wig revamp service.", 15000, "From NGN 15,000", 120, HairImage),
        S(70, "Wig styling", "Wigs", "Wig styling service.", 12000, "From NGN 12,000", 90, HairImage)
    ];

    private static Service S(int id, string name, string category, string description, decimal price, string priceLabel, int duration, string imageUrl) =>
        new()
        {
            ServiceId = id,
            ServiceName = name,
            Category = category,
            Description = description,
            Price = price,
            PriceLabel = priceLabel,
            Duration = duration,
            ImageUrl = imageUrl
        };
}

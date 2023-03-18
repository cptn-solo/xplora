using System.IO;

namespace Assets.Scripts.Data
{
    public enum TerrainPOI
    {
        NA = 0,
        PowerSource = 100,
        HPSource = 200,
        WatchTower = 300,
    }

    public enum TerrainAttribute // атрибуты клеток игрового поля
    {
        NA = 0,
        Bush,       //Кустарник
        Frowers,    //Цветы
        Mushrums,   //Грибы
        Trees,      //Деревья
        River,      //Река
        Path,       //Тропа
    }
}
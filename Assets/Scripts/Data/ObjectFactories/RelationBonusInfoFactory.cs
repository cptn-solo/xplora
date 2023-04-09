namespace Assets.Scripts.Data
{
    public static class RelationBonusInfoFactory
    {
        public static RelationBonusInfo CreateWithStringParams(string[] rawValues)
        {
            var buffer = ListPool<HeroKind>.Get();
            var ret = new RelationBonusInfo
            {
                Bonus = rawValues[0].ParseIntValue(0, true)
            };

            for (var i = 1; i < rawValues.Length; i++)
                buffer.Add(rawValues[i].HeroKindByName());                    

            var retval = buffer.ToArray();

            ListPool<HeroKind>.Add(buffer);

            ret.TargetKinds = retval;

            return ret;
        }

    }
}
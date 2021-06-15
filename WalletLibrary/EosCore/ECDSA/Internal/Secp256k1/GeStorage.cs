namespace WalletLibrary.EosCore.ECDSA.Internal.Secp256k1
{
    internal class GeStorage
    {
        public FeStorage X;
        public FeStorage Y;

        public GeStorage()
        {
            X = new FeStorage();
            Y = new FeStorage();
        }
    }
}
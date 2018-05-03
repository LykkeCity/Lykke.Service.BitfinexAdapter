namespace Lykke.Service.BitfinexAdapter.Core.Throttling
{
    public interface IThrottling
    {
        bool NeedThrottle(string instrument);
    } 
    
}

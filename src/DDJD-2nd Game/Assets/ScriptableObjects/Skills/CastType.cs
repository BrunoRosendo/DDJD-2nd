public enum CastType
{
    Instant, // Apply cooldown instantly, shoot immediately
    Charge, // Apply cooldown and shoot after release
    Hold, // Apply cooldown after release and shoot while holding
    Spawn, // Apply cooldown and spawn object after release
}

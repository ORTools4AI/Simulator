// Modified for ORTools4AI.Simulator.

namespace ORTools4AI.Simulator.extensions.supplychain.ddmrp;

// DDMRP inventory buffer zones.  All quantities in the same time unit as Adu × LeadTime.
// Use FromZones() to bypass the formula and set zone sizes directly.
internal class DdmrpBuffer
{
    private double? _redZoneOverride;
    private double? _yellowZoneOverride;

    internal double Adu { get; set; }              // average usage per time unit
    internal double LeadTime { get; set; }          // replenishment lead time
    internal double LeadTimeFactor { get; set; } = 0.5;    // F_L: 0.20–1.0 (Ptak & Smith)
    internal double VariabilityFactor { get; set; } = 0.5; // F_V: 0.20–1.0
    internal double GreenZone { get; set; }                // min order batch / cycle stock

    // Ptak & Smith (2016) Formula 1 — factor-based
    internal double RedZoneBase   => Adu * LeadTime * LeadTimeFactor;
    internal double RedZoneSafety => RedZoneBase * VariabilityFactor;

    internal double RedZone    => _redZoneOverride    ?? RedZoneBase + RedZoneSafety;
    internal double YellowZone => _yellowZoneOverride ?? Adu * LeadTime;
    internal double TopOfRed   => RedZone + YellowZone;   // reorder trigger (TOY)
    internal double TopOfGreen => TopOfRed + GreenZone;   // order-up-to (TOG)

    // Lee & Reem (2019) Formula 2: replaces subjective factors with measurable CVs.
    // CvDemand = σ_d / μ_d,  CvLeadTime = σ_LT / μ_LT
    internal double RedZoneLeeReem(double cvDemand, double cvLeadTime)
    {
        double safety = Math.Sqrt(1.0 + cvDemand * cvDemand + cvLeadTime * cvLeadTime);
        return Adu * (1.02 * Math.Sqrt(LeadTime) + 1.15) * safety;
    }

    // Factory: set zone sizes directly (experimental design as in Fernandes et al. 2025).
    internal static DdmrpBuffer FromZones(double redZone, double yellowZone, double greenZone)
    {
        var b = new DdmrpBuffer { GreenZone = greenZone };
        b._redZoneOverride    = redZone;
        b._yellowZoneOverride = yellowZone;
        return b;
    }
}

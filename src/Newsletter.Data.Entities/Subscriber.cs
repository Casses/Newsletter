namespace Newsletter.Data.Entities;

public class Subscriber : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime? LastNotifiedAt { get; set; }
    public DateTime? LastEmailSentAt { get; set; }
    public DateTime? LastSmsSentAt { get; set; }
    
    // Notification preferences
    public bool PrefersEmail { get; set; } = true;
    public bool PrefersSms { get; set; }
    public bool PrefersPush { get; set; }
    public string? PushToken { get; set; }
    
    // Geographic preferences
    public string? PreferredCity { get; set; }
    public string? PreferredState { get; set; }
    public string? PreferredCountry { get; set; }
    public double? PreferredRadiusMiles { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Category and tag preferences
    public string? PreferredCategories { get; set; } // Comma-separated list of categories
    public ICollection<SubscriberTag> TagPreferences { get; set; } = new List<SubscriberTag>();
    
    // Subscription metadata
    public string? SubscriptionSource { get; set; } // e.g., "Website", "Event Registration", "Manual"
    public DateTime? UnsubscribedAt { get; set; }
    public string? UnsubscribeReason { get; set; }
    
    // Verification
    public bool IsEmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }
    
    public string? Notes { get; set; }
} 
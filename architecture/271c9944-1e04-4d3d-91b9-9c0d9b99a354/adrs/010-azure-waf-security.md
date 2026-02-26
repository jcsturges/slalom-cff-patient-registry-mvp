# ADR-010: Azure Web Application Firewall for Security

**Status:** Accepted  
**Date:** 2024-01-15

## Context

The NGR system handles Protected Health Information (PHI) and requires:
- Protection against OWASP Top 10 vulnerabilities
- DDoS mitigation
- SSL/TLS termination
- Web traffic filtering
- Compliance with HIPAA security requirements
- Defense in depth for healthcare data

Requirements include:
- Zero critical security vulnerabilities at launch
- Penetration testing compliance
- SOC 2 Type 2 / HITRUST certification support

The RFP tech stack mandates Azure Web Application Firewall.

## Decision

We will use **Azure Web Application Firewall (WAF)** on Application Gateway v2:

### Configuration
- **SKU**: WAF_v2
- **Mode**: Prevention (block malicious requests)
- **Rule Set**: OWASP 3.2 (Core Rule Set)
- **Custom Rules**: Rate limiting, geo-blocking (if required)

### Protection Layers
1. **OWASP Core Rules**
   - SQL Injection protection
   - Cross-Site Scripting (XSS) prevention
   - Local File Inclusion blocking
   - Remote Code Execution prevention

2. **Bot Protection**
   - Known bad bot blocking
   - Crawler management

3. **Rate Limiting**
   - 1000 requests/minute per IP
   - 100 login attempts/minute per IP

4. **Custom Rules**
   - Block requests from known malicious IPs
   - Require specific headers for API access

### SSL/TLS Configuration
- Minimum TLS 1.2
- Strong cipher suites only
- HSTS enabled (max-age=31536000)

## Consequences

### Positive
- **OWASP Protection**: Comprehensive coverage of common vulnerabilities
- **Managed Service**: Microsoft maintains rule updates
- **Logging**: Detailed logs for security analysis
- **Compliance**: Supports HIPAA and SOC 2 requirements
- **Performance**: Minimal latency impact (~1-2ms)
- **Integration**: Native Azure monitoring and alerting

### Negative
- **False Positives**: Legitimate requests may be blocked
- **Cost**: WAF_v2 adds significant monthly cost
- **Complexity**: Rule tuning requires security expertise
- **Debugging**: Blocked requests can be hard to diagnose

### Mitigations
- Start in Detection mode, tune rules, then switch to Prevention
- Create exclusions for known false positives
- Implement detailed logging for blocked requests
- Document rule customizations for audit purposes

## Alternatives Considered

### Cloudflare WAF
- **Rejected**: Not in approved tech stack
- Would require DNS changes
- Additional vendor relationship

### Azure Front Door with WAF
- **Rejected**: Overkill for single-region deployment
- Higher cost for current scale
- Could be considered for future global expansion

### No WAF (rely on application security only)
- **Rejected**: Does not meet defense-in-depth requirements
- Insufficient for HIPAA compliance
- Higher risk of successful attacks

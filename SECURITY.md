# Security Policy

## Supported Versions

We are committed to addressing security vulnerabilities in the following versions:

| Version | Supported          |
| ------- | ------------------ |
| 0.x     | :white_check_mark: |

## Reporting a Vulnerability

**Please do not report security vulnerabilities through public GitHub issues.**

Instead, please report security vulnerabilities by email to:

**security@elliveny.com**

You should receive a response within 48 hours. If for some reason you do not, please follow up via GitHub to ensure we received your original message.

Please include the following information in your report:

- Type of vulnerability (e.g., buffer overflow, SQL injection, cross-site scripting, etc.)
- Full paths of source file(s) related to the manifestation of the issue
- The location of the affected source code (tag/branch/commit or direct URL)
- Any special configuration required to reproduce the issue
- Step-by-step instructions to reproduce the issue
- Proof-of-concept or exploit code (if possible)
- Impact of the issue, including how an attacker might exploit the issue

This information will help us triage your report more quickly.

## Disclosure Policy

When we receive a security bug report, we will:

1. Confirm the problem and determine the affected versions
2. Audit code to find any similar problems
3. Prepare fixes for all supported releases
4. Release new security fix versions as soon as possible

## Security Update Communications

Security updates will be announced via:

- GitHub Security Advisories
- Release notes in CHANGELOG.md
- GitHub Releases page

## Security Best Practices for Users

When using this library, please follow these security best practices:

1. **Keep the library updated**: Always use the latest version to benefit from security patches
2. **Secure API keys**: Never commit API keys to source control
3. **Use HTTPS**: Always connect to Valhalla servers over HTTPS in production
4. **Validate input**: Validate all user-provided coordinates and parameters before passing to the client
5. **Monitor logs**: Review logs regularly for unusual patterns, but note that API keys are automatically redacted
6. **Configure timeouts**: Set appropriate timeout values to prevent resource exhaustion
7. **Review dependencies**: Keep all dependencies up to date with security patches

## Known Security Considerations

### DoS Protection

This library includes built-in DoS protection:
- Response size limit: 10MB (configurable)
- Timeout configuration required
- Cancellation token support

### API Key Security

- API keys are automatically redacted from logs
- Keys are applied per-request
- Keys are never persisted in memory longer than necessary

### Input Validation

The library validates:
- Coordinate ranges (latitude: -90 to 90, longitude: -180 to 180)
- Request parameter constraints
- Response size limits

However, additional application-level validation is recommended before accepting user input.

## Credits

We would like to thank the following individuals for responsibly disclosing security issues:

- (List will be maintained as vulnerabilities are reported and fixed)

---

**Last Updated:** February 8, 2026

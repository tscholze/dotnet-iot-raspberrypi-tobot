# Contributing to Tobot

Thank you for your interest in contributing to Tobot! This document provides guidelines and instructions for contributing.

## 🎯 Project Scope & Intent

**Important Notice:** This project is a personal learning and experimentation platform. It is **not intended as a production-ready product or a general-purpose solution** for others.

### What This Means

- **Personal Project** - Built primarily for my own robotics experiments and learning
- **No Custom Features** - I have no intention to implement custom features on request
- **No Support Obligations** - I don't provide technical support or troubleshooting assistance
- **No Guarantees** - Code may change dramatically, break, or be abandoned at any time

### Collaboration Is Still Welcome!

Despite the personal nature of this project:

- ✅ **Feel free to fork** - Take the code and make it your own
- ✅ **Contributions accepted** - If you fix bugs or add features you need, PRs are welcome
- ✅ **Learn from it** - Use it as a reference for your own projects
- ✅ **Share your work** - Show off what you've built with it

### If You Know What You're Doing

If you're experienced with .NET IoT development and want to contribute:

- Fork the repo and experiment freely
- Submit PRs for bug fixes or enhancements
- Don't expect detailed code reviews or merge guarantees
- Understand that your contribution might not align with my personal goals

**Bottom line:** This is a playground, not a product. Use it, learn from it, contribute to it—but don't expect it to serve your specific needs out of the box.

## 🤝 Ways to Contribute

We welcome contributions in many forms:

- 🐛 **Bug Reports** - Found an issue? Let us know!
- 💡 **Feature Requests** - Have an idea? We'd love to hear it!
- 📝 **Documentation** - Improve guides, fix typos, add examples
- 🎨 **Code Examples** - Share your robot projects
- 💻 **Code Contributions** - Fix bugs, add features, improve performance
- 🧪 **Testing** - Help verify the library on different Pi models

## 🚀 Getting Started

### Prerequisites

- Raspberry Pi with Explorer HAT (for hardware testing)
- .NET 9 SDK installed
- Git for version control
- Familiarity with C# and GitHub workflow

### Development Setup

1. **Fork the repository**
   ```bash
   # Fork on GitHub, then clone your fork
   git clone https://github.com/yourusername/tobot.git
   cd tobot
   ```

2. **Create a branch**
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b fix/your-bug-fix
   ```

3. **Build and test**
   ```bash
   dotnet build
   dotnet run --project Tobot check
   ```

## 📋 Contribution Guidelines

### Code Style

We follow standard C# conventions:

- **Naming Conventions**
  - PascalCase for classes, methods, properties
  - camelCase for local variables and parameters
  - _camelCase for private fields
  - Use descriptive names

- **Documentation**
  - All public members must have XML documentation comments
  - Include `<summary>`, `<param>`, and `<returns>` tags
  - Provide clear, concise descriptions

- **Code Organization**
  - Keep files focused and small (~100 lines max)
  - One class per file
  - Group related functionality in packages
  - Use regions sparingly, prefer good structure

- **Modern C# Features**
  - Use C# 13 features where appropriate
  - Prefer pattern matching over if-else chains
  - Use using statements for IDisposable
  - Leverage nullable reference types

### Example Code Style

```csharp
namespace Tobot.Device.ExplorerHat.Motor;

/// <summary>
/// Represents a single motor with H-bridge control.
/// </summary>
public class Motor : IDisposable
{
    private readonly GpioController _controller;
    private readonly MotorPinMapping _pins;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Motor"/> class.
    /// </summary>
    /// <param name="controller">The GPIO controller.</param>
    /// <param name="pins">The pin mapping for this motor.</param>
    public Motor(GpioController controller, MotorPinMapping pins)
    {
        _controller = controller;
        _pins = pins;
    }
    
    /// <summary>
    /// Sets the motor speed and direction.
    /// </summary>
    /// <param name="speed">Speed from -100 (full backward) to +100 (full forward).</param>
    public void SetSpeed(double speed)
    {
        var clampedSpeed = Math.Clamp(speed, -100, 100);
        // Implementation...
    }
    
    /// <summary>
    /// Releases all resources used by the motor.
    /// </summary>
    public void Dispose()
    {
        // Cleanup...
    }
}
```

## 🐛 Reporting Bugs

### Before Submitting

1. **Search existing issues** - Your bug might already be reported
2. **Update to latest** - Ensure you're on the latest version
3. **Verify on hardware** - Test on actual Explorer HAT if possible

### Bug Report Template

```markdown
**Description**
Clear description of the bug

**To Reproduce**
Steps to reproduce:
1. Initialize ExplorerHat
2. Call method X
3. Observe error Y

**Expected Behavior**
What should happen

**Actual Behavior**
What actually happens

**Environment**
- Tobot Version: 
- .NET Version: 
- Raspberry Pi Model: 
- OS Version: 
- Explorer HAT Version: 

**Code Sample**
\`\`\`csharp
// Minimal code to reproduce
\`\`\`

**Error Messages**
```
Full error message and stack trace
```
```

## 💡 Feature Requests

We love new ideas! When proposing features:

1. **Check existing requests** - Avoid duplicates
2. **Describe the use case** - Why is this needed?
3. **Provide examples** - Show how it would work
4. **Consider scope** - Is it appropriate for this project?

### Feature Request Template

```markdown
**Feature Description**
What feature are you proposing?

**Use Case**
Why is this needed? What problem does it solve?

**Proposed API**
\`\`\`csharp
// Example of how it might look
hat.NewFeature.DoSomething();
\`\`\`

**Alternatives Considered**
Other ways to achieve this

**Additional Context**
Any other information
```

## 💻 Code Contributions

### Pull Request Process

1. **Create an issue first** - Discuss major changes before coding
2. **Follow code style** - Match existing patterns
3. **Add tests** - If applicable (we're building test infrastructure)
4. **Update documentation** - Keep docs in sync with code
5. **Build successfully** - Ensure `dotnet build` passes
6. **Keep it focused** - One feature/fix per PR

### PR Checklist

Before submitting:

- [ ] Code follows project style guidelines
- [ ] All public members have XML documentation
- [ ] Build succeeds without warnings
- [ ] Tested on actual hardware (if possible)
- [ ] Documentation updated (README, inline comments)
- [ ] Commit messages are clear and descriptive
- [ ] PR description explains what and why

### Commit Message Format

```
<type>: <subject>

<body>

<footer>
```

**Types:**
- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation changes
- `style:` Code style (formatting, no logic change)
- `refactor:` Code restructuring
- `perf:` Performance improvement
- `test:` Adding tests
- `chore:` Build/tooling changes

**Example:**
```
feat: Add PWM motor speed control

Implement hardware PWM for precise motor speed control using
the PwmChannel class. This allows for smooth speed transitions
and better motor control.

Closes #42
```

## 📦 Package Structure

When adding new functionality:

### Adding to Existing Package

If your feature fits an existing package (Motor, LED, Analog, Digital, Touch):

```
Tobot.Device/ExplorerHat/
??? [Package]/
    ??? YourNewClass.cs
    ??? YourNewCollection.cs  (if needed)
```

### Creating New Package

For significant new functionality:

```
Tobot.Device/ExplorerHat/
??? NewFeature/
    ??? NewFeature.cs
    ??? NewFeatureCollection.cs
    ??? NewFeatureConfig.cs  (if needed)
```

Update `ExplorerHat.cs` to integrate:
```csharp
using Tobot.Device.ExplorerHat.NewFeature;

public class ExplorerHat
{
    public NewFeatureCollection NewFeature { get; }
    
    public ExplorerHat()
    {
        // Initialize new feature
        NewFeature = new NewFeatureCollection(...);
    }
}
```

## 🧪 Testing

We're building a comprehensive test suite. When it's ready:

- Write unit tests for new features
- Ensure existing tests pass
- Add integration tests for hardware interaction
- Document test setup requirements

## 📚 Documentation

### Code Documentation

- Use XML documentation comments on all public members
- Be clear and concise
- Include examples for complex APIs
- Document exceptions that can be thrown

### README Updates

When adding features, update:
- Main [README.md](README.md) - Add to features list
- [Tobot/README.md](Tobot/README.md) - Add usage examples
- [Tobot.Device/ExplorerHat/README.md](Tobot.Device/ExplorerHat/README.md) - API reference

### Examples

Add examples to:
- `Tobot/Program.cs` - Interactive demos
- `Tobot.Device/ExplorerHat/ExplorerHatExample.cs` - Code samples
- Documentation files - Usage scenarios

## 🏗️ Architecture Decisions

### Design Principles

1. **Package by Feature** - Related components together
2. **Self-Contained** - No cross-package dependencies
3. **Clean APIs** - Intuitive, discoverable interfaces
4. **Resource Safety** - IDisposable everywhere
5. **Modern C#** - Use latest language features

### API Design

- Favor composition over inheritance
- Use collections for related items
- Provide both indexed and named access
- Support both sync and async patterns
- Make common tasks easy, complex tasks possible

## 💬 Questions?

- Open a [Discussion](https://github.com/yourusername/tobot/discussions) for general questions
- Join our community chat (link when available)
- Check existing [Issues](https://github.com/yourusername/tobot/issues) and [PRs](https://github.com/yourusername/tobot/pulls)

## 🏆 Recognition

Contributors will be:
- Listed in the project's contributor list
- Mentioned in release notes
- Credited in documentation

Significant contributions may earn you:
- Commit access to the repository
- Mention on the project homepage
- Our eternal gratitude! 🙏

## 📜 Code of Conduct

### Our Pledge

We are committed to providing a welcoming and inclusive environment for everyone.

### Expected Behavior

- Be respectful and considerate
- Welcome newcomers and help them learn
- Accept constructive criticism gracefully
- Focus on what's best for the community
- Show empathy towards others

### Unacceptable Behavior

- Harassment, discrimination, or hostile behavior
- Trolling, insulting comments, or personal attacks
- Publishing others' private information
- Other conduct inappropriate in a professional setting

### Enforcement

Violations may result in temporary or permanent ban from the project.

## 🙏 Thank You!

Every contribution, no matter how small, makes Tobot better!

Your time and effort are appreciated by the entire community.

**Happy coding, and welcome to the Tobot family!** 🤖

---

<div align="center">

Questions? [Open an Issue](https://github.com/yourusername/tobot/issues) | [Start a Discussion](https://github.com/yourusername/tobot/discussions)

</div>

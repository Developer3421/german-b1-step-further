# License Test and Documentation

## Purpose
This file demonstrates and tests the understanding of the custom license
applied to the German B1. Step Further repository.

## License Structure

The repository uses a **dual-license** approach:

### 1. Source Code (MIT License)
✅ **PERMITTED:**
- Use the source code for any purpose
- Modify the source code
- Distribute the source code
- Create derivative works based on the code
- Use commercially
- Use privately

**Example permitted uses:**
```
✅ Fork the repository and modify the C# code
✅ Use the Avalonia UI code patterns in your own project
✅ Learn from and adapt the program structure
✅ Create a similar application for learning French or Spanish
✅ Use the code in commercial products
```

### 2. Graphical Style & Resources (All Rights Reserved)
❌ **RESTRICTED:**
- Icons directory (all .png files)
- Resources directory (all .axaml content files)
- UI/UX design elements
- Visual styles and branding
- Educational content

**Example restricted uses:**
```
❌ Copy AddTab.png, CloseButton.png, or other icons
❌ Reuse the Part1Topic1Content.axaml resource files
❌ Copy the visual design/layout of the application
❌ Extract and use the German learning content
❌ Create a similar-looking application using the same graphical style
```

## Test Cases

### Test Case 1: Using Source Code ✅
**Scenario:** A developer wants to create a Spanish learning app using
similar code structure.

**Permitted Actions:**
- Copy and adapt Program.cs logic
- Reuse the MainWindow.axaml.cs code patterns
- Learn from and implement similar Models and Services
- Use the same Avalonia UI framework approach

**Result:** ✅ ALLOWED under MIT License

---

### Test Case 2: Reusing Icons ❌
**Scenario:** A developer wants to use the app5 icon.png or ForwardButton.png
in their own project.

**Permitted Actions:**
- NONE - these are graphical resources

**Required Action:**
- Contact repository owner for explicit written permission

**Result:** ❌ NOT ALLOWED without permission

---

### Test Case 3: Adapting Educational Content ❌
**Scenario:** A developer wants to extract the German B1 topics from
Part1Topic1Content.axaml for their own learning website.

**Permitted Actions:**
- NONE - this is educational content in restricted resources

**Required Action:**
- Contact repository owner for explicit written permission

**Result:** ❌ NOT ALLOWED without permission

---

### Test Case 4: Learning from Code Architecture ✅
**Scenario:** A developer studies the tab-based navigation system and
implements a similar system in their app with different content and design.

**Permitted Actions:**
- Study the code structure
- Implement similar architectural patterns
- Use different visual assets
- Create different content

**Result:** ✅ ALLOWED under MIT License (as long as no graphical assets
or content are copied)

---

### Test Case 5: Forking and Modifying ✅/❌
**Scenario:** A developer forks the repository to add new features.

**Permitted Actions:**
- Modify all source code files (.cs, .axaml.cs)
- Change program logic and behavior
- Add new functionality

**Restricted Actions:**
- Cannot redistribute the Icons/ folder contents
- Cannot redistribute the Resources/ folder contents
- Must replace graphical assets if distributing
- Must create original educational content

**Result:** ✅ Code changes allowed, ❌ Asset redistribution restricted

---

## Compliance Checklist

When using this repository, ensure you:

- [ ] Only use source code (.cs files, program logic)
- [ ] Do NOT copy any files from Icons/ directory
- [ ] Do NOT copy any files from Resources/ directory
- [ ] Do NOT replicate the visual design/UI appearance
- [ ] Provide attribution for MIT-licensed code (recommended)
- [ ] Create your own graphical assets and content
- [ ] Contact owner if you need to use restricted materials

## Contact for Permissions

For questions or to request permission to use graphical assets or resources:
- Check repository owner's contact information
- Create an issue in the GitHub repository
- Provide detailed explanation of intended use

## License Verification

Last Updated: January 2026
License File: LICENSE (in repository root)
License Type: Dual License (MIT for code, All Rights Reserved for assets)

---

## Notes for Developers

If you're using this code:
1. Read the full LICENSE file carefully
2. Understand the difference between code and assets
3. Replace all Icons/ directory contents with your own
4. Replace all Resources/ directory contents with your own
5. You're free to use the code structure and logic

This ensures compliance with both licenses and respects the intellectual
property rights of the original creators while allowing code reuse.

# open-world-assets
Unity Assets for an Open World Game

## Getting Started
- [Branching Structure](#branching-structure)
- [Setup](#setup)

### Braning Structure
- _main_
  - The, permanent, main branch for "Finalalized" Assets
- _qa_
  - Permanent Branch, which contains features which have been tested, but needs outside review.
  - Assets at this stage should be functional, and just need to be peer reviewed for the looks and feel of the desired feature.
  - This is the last stage before merging with _main_.
- _playground_
  - Permanent Branch for playing around with new features which have not been finalized
  - _playground_ is branched off _qa_, and rebased off the branch when issues are merged
- _issue-#_
  - Temorary Branches for handling issues made to this repo

  ### Setup
  - Download Unity
    - Packages
      - Input System

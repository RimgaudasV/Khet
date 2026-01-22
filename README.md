# Khet 2.0 Game Agent Using Alpha–Beta Search

This repository contains a course project developed for the **Software Development** study program at **Vilnius University**.

The goal of this project is to design and implement an AI agent for the board game **Khet 2.0**, using the **Minimax algorithm with Alpha–Beta pruning**, and to experimentally evaluate the efficiency of the agent.  
The experimental results are used and analyzed in the accompanying course paper.

---

## 📌 Project Overview

**Khet 2.0** is a deterministic, two-player, perfect-information board game where players attempt to eliminate the opponent’s Pharaoh using laser reflections from mirrored pieces.  
Due to a large branching factor and complex interactions between pieces, the game presents a non-trivial search problem for classical AI algorithms.

In this project:

- A full **Khet 2.0 game engine** was implemented
- **Minimax search**
- **Alpha–Beta pruning**
- Custom **heuristic evaluation function**, based on:
  - material value of pieces
  - positional pressure on the opponent’s Pharaoh
  - interaction with the Sphinx laser axes

---

To launch frontend part, use ``npm start`` in "khetweb" project
To launch backend part, use ``dotnetrun`` in "KhetApi" project

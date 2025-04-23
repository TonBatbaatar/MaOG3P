# MaOG3P: Many-Objective Grammar-Guided Genetic Programming for Program Synthesis

This repository contains **MaOG3P**, a research tool developed to support the experiments presented in the publication:  
**["Enhancing Program Synthesis with Large Language Models Using Many-Objective Grammar-Guided Genetic Programming"](https://doi.org/10.3390/a17070287)**.


MaOG3P is built as an **extension of HeuristicLab**, implementing a customised many-objective genetic programming algorithm designed specifically for grammar-obeying program synthesis. It provides the components necessary to reproduce the results in the paper, including novel multi-objective optimisation algorithms and problem definitions.

---

## 📖 Overview

MaOG3P enables evolving candidate programs using **many-objective optimisation** techniques that go beyond traditional single-objective approaches. It introduces objectives such as:

- **Error rate** (fitness of the candidate program)
- **Code similarity** (to guide evolution toward more interpretable or desirable structures)

---

## 🧠 Key Features

- ✅ **Customised Many-Objective Genetic Programming Algorithm**  
  Tailored for evolving solutions based on multiple objectives, like traditional error-rated evaluation and structural similarity.

- ✅ **Grammar-Guided Program Synthesis Support**  
  Leverages grammar-based rules to evolve syntactically valid programs only.

- ✅ **LLM Integration for Program Seeding**  
  Supports seeding the initial population using LLM-generated code to bootstrap the search.

- ✅ **Pluggable Problem Definitions**  
  Includes multi-objective problem setups specifically designed for the synthesis experiments in the publication.

- ✅ **HeuristicLab Extension**  
  Fully integrates with HeuristicLab for GUI-based experimentation and reuse of existing operators.

---

## 📦 Installation

1. Clone this repository:
   ```bash
   git clone https://github.com/Tonbatbaatar/MaOG3O.git
2. Download and install HeuristicLab.
3. Open the MaOG3P solution in Visual Studio and build the project (ensure HeuristicLab references are set correctly).
4. Launch HeuristicLab and import the compiled plugins.

---

## 🚀 Running Experiments
1. Select the MaOG3P algorithm and assign it the appropriate multi-objective synthesis problem.
2. Configure parameters.
3. Run the experiment and observe objective trends, solution quality.

---

## 📄 Publication
If you use this tool in your research, please cite:
@article{tao2024enhancing,
  title={Enhancing Program Synthesis with Large Language Models Using Many-Objective Grammar-Guided Genetic Programming},
  author={Tao, Ning and Ventresque, Anthony and Nallur, Vivek and Saber, Takfarinas},
  journal={Algorithms},
  volume={17},
  number={7},
  pages={287},
  year={2024},
  publisher={MDPI}
}

---

## 🧪 Module Info
- Project: MaOG3P
- Based on: HeuristicLab framework
- Affiliation: University College Dublin
- Purpose: Research prototype for evolving programs using many-objective optimisation

---

### 🤝 License
This project is intended for academic and research use only.

# ReferenceParser
Input string list to parse for possible reference numbers and associated HODs.

Required reference -  [ReferenceChecker](https://github.com/finct-bdapp-development/ReferenceChecker.git)
                      [DMBTools](https://github.com/finct-bdapp-development/DMBTools.git)

ReferenceParser will parse the provided string list and output the identified references into a new sting list that the Checker will then parse the check that these identified reference numbers are of vaild format.

ReferenceParser makes use of m_GeneralTools.checkNumeric(string) which is the reason for the inclusion of DMBTools reference.

# MatchGenerator

This is a package which generated a Match function on your enum types which are tagged with the `[Match]` attribute.
This match function forces you to handle all cases of your enum type.

It comes in two forms:
- The no-return version, which resembles switch statement.
- The return version, which resembles switch expression.

Either way they are both exhaustive and will force you to handle all cases.
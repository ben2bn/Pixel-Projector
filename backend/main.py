from dataclasses import dataclass
from typing import List

from langchain.messages import AIMessage
from langchain.tools import tool
from langchain_ollama import ChatOllama
from langgraph.prebuilt import ToolRuntime

mixer_prompt = """You are a worker in an office who mixes paint from the three primary colors.
You need to go to the resource station to collect the three basic paints. Then you can mix them in certain quantities.

You have access to the following tools:
- go_collect_paint: use this to collect a specific paint from the resource station
- mix_paints: use this to mix together the paints that you have

Only the following combinations work:
- red and green together make yellow
- red and blue together make magenta
- green and blue together make cyan

Look at the list of final colors you have to plan out how you will make them. There are only pure red, green and blue colors at the resource station.
"""

owned_red_paint: int = 0
owned_green_paint: int = 0
owned_blue_paint: int = 0


@dataclass
class ResponseFormat:
    """Response schema for the agent model."""


@tool
def go_collect_paint(color: str) -> str:
    """Collect paints from resource station and add them to your inventory."""
    global owned_red_paint, owned_green_paint, owned_blue_paint
    if color == "red":
        owned_red_paint += 1
    elif color == "green":
        owned_green_paint += 1
    elif color == "blue":
        owned_blue_paint += 1
    return f"You have collected paint. You now have {owned_red_paint} red paint, {owned_green_paint} green paint, and {owned_blue_paint} blue paint."


@tool
def mix_paints(color_1: str, color_2: str) -> str:
    """Mix together the paints that you have collected from the resource station to create new colors."""
    global owned_red_paint, owned_green_paint, owned_blue_paint
    if (color_1 == "red" and color_2 == "green" )or (color_1 == "green" and color_2 == "red"):
        owned_red_paint -= 1
        owned_green_paint -= 1
        return "You have obtained yellow but lost one red and one green paint!"
    elif (color_1 == "red" and color_2 == "blue") or (color_1 == "blue" and color_2 == "red"):
        owned_red_paint -= 1
        owned_blue_paint -= 1
        return "You have obtained magenta but lost one red and one blue paint!"
    elif (color_1 == "green" and color_2 == "blue") or (color_1 == "blue" and color_2 == "green"):
        owned_green_paint -= 1
        owned_blue_paint -= 1
        return "You have obtained cyan but lost one green and one blue paint!"
    return ""

# @tool
# def saturate_colors(color: str, runtime: ToolRuntime[Context]) -> str:
#     """Change the saturation of a mixed color."""
#     return "Your red is now dark."

llm = ChatOllama(
    model="qwen3.5:2b",
    validate_model_on_init=True,
    temperature=0,
    num_gpu=0
).bind_tools([go_collect_paint, mix_paints])

messages = [
    ("system", mixer_prompt),
    ("human", f"Obtain yellow. You have {owned_red_paint} red paint, {owned_green_paint} green paint, and {owned_blue_paint} blue paint."),
]

result = llm.invoke(messages)

if isinstance(result, AIMessage) and result.tool_calls:
    print(result)

messages = [
    ("system", mixer_prompt),
    ("human", f"Obtain yellow. You have {owned_red_paint} red paint, {owned_green_paint} green paint, and {owned_blue_paint} blue paint."),
]

result = llm.invoke(messages)

if isinstance(result, AIMessage) and result.tool_calls:
    print(result)
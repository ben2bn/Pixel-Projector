from flask import Flask, jsonify, request

from langchain.messages import AIMessage
from langchain.tools import tool
from langchain_google_genai import ChatGoogleGenerativeAI

import dotenv

dotenv.load_dotenv()

mixer_prompt = """You are a worker in an office who mixes colors from the three primary colors, red, green and blue.
You can collect the three primary colors at the central resource station. Then you can mix them in certain quantities to obtain either yellow, magenta or cyan.

You have access to the following tools:
- go_collect_paint: use this to collect a specific paint color from the resource station
- mix_paints: use this to mix together the paints that you have to obtain new colors

Only the following mixes work:
- Mix red and green to obtain yellow
- Mix red and blue obtain magenta
- Mix green and blue obtain cyan

You must mix together colors to obtain yellow, magenta or cyan.

Look at the list of colors that you already have to choose how the obtain the target color.

Put all the required tools you would use in a single answer.
"""

owned_red_paint: int = 0
owned_green_paint: int = 0
owned_blue_paint: int = 0


# @tool
# def saturate_colors(color: str, runtime: ToolRuntime[Context]) -> str:
#     """Change the saturation of a mixed color."""
#     return "Your red is now dark."

@tool
def go_collect_paint(color: str) -> str:
    """Collect primary paints from resource station and add them to your inventory."""
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
    """Mix together the paints that you already own to obtain yellow, magenta or cyan colors."""
    global owned_red_paint, owned_green_paint, owned_blue_paint
    if (color_1 == "red" and color_2 == "green") or (color_1 == "green" and color_2 == "red"):
        owned_red_paint -= 1
        owned_green_paint -= 1
        return "You have obtained yellow!"
    elif (color_1 == "red" and color_2 == "blue") or (color_1 == "blue" and color_2 == "red"):
        owned_red_paint -= 1
        owned_blue_paint -= 1
        return "You have obtained magenta!"
    elif (color_1 == "green" and color_2 == "blue") or (color_1 == "blue" and color_2 == "green"):
        owned_green_paint -= 1
        owned_blue_paint -= 1
        return "You have obtained cyan!"
    return ""


mixer_llm = ChatGoogleGenerativeAI(
    model="gemma-4-26b-a4b-it",
    temperature=1.0,  # Gemini 3.0+ defaults to 1.0
    max_tokens=None,
    timeout=None,
    max_retries=2
).bind_tools([go_collect_paint, mix_paints])

app = Flask(__name__)


@app.route("/mixer")
def run_mixer():
    target_color = request.args.get("color")
    messages = [("system", mixer_prompt),
                ("human",
                 f"You need to obtain {target_color}. You have {owned_red_paint} red paint, {owned_green_paint} green paint, and {owned_blue_paint} blue paint."),
                ]

    result = mixer_llm.invoke(messages)

    if isinstance(result, AIMessage) and result.tool_calls:
        print(result.content)
        print(result.tool_calls)
        print(result.usage_metadata)

    return jsonify(
        result=result.tool_calls,
    )

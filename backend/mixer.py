from langchain_core.tools import tool
from langchain_google_genai import ChatGoogleGenerativeAI

mixer_owned_red_paint: int = 0
mixer_owned_green_paint: int = 0
mixer_owned_blue_paint: int = 0


@tool
def go_collect_paint(color: str) -> str:
    """Collect primary paints from resource station and add them to your inventory."""
    global mixer_owned_red_paint, mixer_owned_green_paint, mixer_owned_blue_paint
    if color == "red":
        mixer_owned_red_paint += 1
    elif color == "green":
        mixer_owned_green_paint += 1
    elif color == "blue":
        mixer_owned_blue_paint += 1
    return f"You have collected paint. You now have {mixer_owned_red_paint} red paint, {mixer_owned_green_paint} green paint, and {mixer_owned_blue_paint} blue paint."


@tool
def mix_paints(color_1: str, color_2: str) -> str:
    """Mix together the paints that you already own to obtain yellow, magenta or cyan colors."""
    global mixer_owned_red_paint, mixer_owned_green_paint, mixer_owned_blue_paint
    if (color_1 == "red" and color_2 == "green") or (color_1 == "green" and color_2 == "red"):
        mixer_owned_red_paint -= 1
        mixer_owned_green_paint -= 1
        return "You have obtained yellow!"
    elif (color_1 == "red" and color_2 == "blue") or (color_1 == "blue" and color_2 == "red"):
        mixer_owned_red_paint -= 1
        mixer_owned_blue_paint -= 1
        return "You have obtained magenta!"
    elif (color_1 == "green" and color_2 == "blue") or (color_1 == "blue" and color_2 == "green"):
        mixer_owned_green_paint -= 1
        mixer_owned_blue_paint -= 1
        return "You have obtained cyan!"
    return ""


mixer_llm = ChatGoogleGenerativeAI(
    model="gemini-3-flash-preview",
    temperature=1.0,
    thinking_level="minimal"
).bind_tools([go_collect_paint, mix_paints])

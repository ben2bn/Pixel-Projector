from langchain_core.tools import tool
from langchain_google_genai import ChatGoogleGenerativeAI

painter_owned_red_paint: int = 0
painter_owned_green_paint: int = 0
painter_owned_blue_paint: int = 0
painter_owned_yellow_paint: int = 0
painter_owned_magenta_paint: int = 0
painter_owned_cyan_paint: int = 0


@tool
def go_collect_paint(color: str) -> str:
    """Collect paints from resource station and add them to your inventory."""
    global painter_owned_red_paint, painter_owned_green_paint, painter_owned_blue_paint, painter_owned_yellow_paint, painter_owned_magenta_paint, painter_owned_cyan_paint
    if color == "red":
        painter_owned_red_paint += 1
    elif color == "green":
        painter_owned_green_paint += 1
    elif color == "blue":
        painter_owned_blue_paint += 1
    elif color == "yellow":
        painter_owned_yellow_paint += 1
    elif color == "magenta":
        painter_owned_magenta_paint += 1
    elif color == "cyan":
        painter_owned_cyan_paint += 1
    return f"You have collected paint. You now have {painter_owned_red_paint} red paint, {painter_owned_green_paint} green paint, {painter_owned_blue_paint} blue paint, {painter_owned_yellow_paint} yellow paint, {painter_owned_magenta_paint} magenta paint, and {painter_owned_cyan_paint} cyan paint."


@tool
def combine_paints(color: str, position: int) -> str:
    """Put the right colored paint into the right position."""
    global painter_owned_red_paint, painter_owned_green_paint, painter_owned_blue_paint, painter_owned_yellow_paint, painter_owned_magenta_paint, painter_owned_cyan_paint
    match color:
        case "red":
            if painter_owned_red_paint <= 0:
                return "You don't have enough red paint"
            painter_owned_red_paint -= 1
            return f"You have put red paint in position {position}."
        case "green":
            if painter_owned_green_paint <= 0:
                return "You don't have enough green paint"
            painter_owned_green_paint -= 1
            return f"You have put green paint in position {position}."
        case "blue":
            if painter_owned_blue_paint <= 0:
                return "You don't have enough blue paint"
            painter_owned_blue_paint -= 1
            return f"You have put blue paint in position {position}."
        case "yellow":
            if painter_owned_yellow_paint <= 0:
                return "You don't have enough yellow paint"
            painter_owned_yellow_paint -= 1
            return f"You have put yellow paint in position {position}."
        case "cyan":
            if painter_owned_cyan_paint <= 0:
                return "You don't have enough cyan paint"
            painter_owned_cyan_paint -= 1
            return f"You have put cyan paint in position {position}."
        case "magenta":
            if painter_owned_magenta_paint <= 0:
                return "You don't have enough magenta paint"
            painter_owned_magenta_paint -= 1
            return f"You have put magenta paint in position {position}."
    return ""


painter_llm = ChatGoogleGenerativeAI(
    model="gemini-3-flash-preview",
    temperature=1.0,
    thinking_level="low"
).bind_tools([go_collect_paint, combine_paints])
